using Godot;

namespace Game.Components
{
    public partial class EdgeCheckComponent : Node3D
    {
        [Export] public float ProbeDistance = 1.5f;
        [Export] public float ProbeHeight = 1.0f;
        [Export] public float RayLength = 3.0f;

        [Export] public uint GroundCollisionMask = 1;

        [Export(PropertyHint.Range, "0,1,0.01")]
        public float MinimumFloorNormalY = 0.6f;

        private CollisionObject3D _ownerCollision;

        public override void _Ready()
        {
            _ownerCollision = Owner as CollisionObject3D;

            if (_ownerCollision == null)
                GD.PushWarning($"{Name}: Owner should be a CollisionObject3D.");
        }

        public bool CanMove(Vector3 worldDirection)
        {
            worldDirection.Y = 0f;

            if (worldDirection.LengthSquared() <= 0.001f)
                return true;

            worldDirection = worldDirection.Normalized();

            Vector3 rayStart =
                GlobalPosition +
                worldDirection * ProbeDistance +
                Vector3.Up * ProbeHeight;

            Vector3 rayEnd =
                rayStart +
                Vector3.Down * RayLength;

            PhysicsRayQueryParameters3D query =
                PhysicsRayQueryParameters3D.Create(
                    rayStart,
                    rayEnd,
                    GroundCollisionMask
                );

            if (_ownerCollision != null)
                query.Exclude = new Godot.Collections.Array<Rid> { _ownerCollision.GetRid() };

            Godot.Collections.Dictionary result =
                GetWorld3D().DirectSpaceState.IntersectRay(query);

            if (result.Count == 0)
                return false;

            Vector3 normal = result["normal"].AsVector3();

            return normal.Y >= MinimumFloorNormalY;
        }
    }
}