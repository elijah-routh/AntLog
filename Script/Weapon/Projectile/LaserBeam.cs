using Godot;

public partial class LaserBeam : RayCast3D
{
    [Export] public MeshInstance3D BeamMesh { get; set; }
    [Export] public float MaxDistance { get; set; } = 30f;
    [Export] public float BeamRadius { get; set; } = 0.15f;

    public override void _Ready()
    {
        Enabled = true;

        if (TargetPosition != Vector3.Zero)
            TargetPosition = TargetPosition.Normalized() * MaxDistance;

        // Important: prevent multiple lasers from sharing the same CylinderMesh resource.
        if (BeamMesh?.Mesh != null)
            BeamMesh.Mesh = (Mesh)BeamMesh.Mesh.Duplicate();
    }

    public override void _Process(double delta)
    {
        ForceRaycastUpdate();

        Vector3 direction = TargetPosition.Normalized();
        float length = MaxDistance;

        if (IsColliding())
        {
            Vector3 localHitPoint = ToLocal(GetCollisionPoint());
            length = localHitPoint.Dot(direction);
        }

        length = Mathf.Clamp(length, 0f, MaxDistance);

        UpdateBeamVisual(length);
    }

    private void UpdateBeamVisual(float length)
    {
        if (BeamMesh == null)
            return;

        Vector3 direction = TargetPosition.Normalized();

        if (direction == Vector3.Zero)
            return;

        if (BeamMesh.Mesh is CylinderMesh cylinder)
        {
            cylinder.Height = length;
            cylinder.TopRadius = BeamRadius;
            cylinder.BottomRadius = BeamRadius;
        }

        BeamMesh.Position = direction * length * 0.5f;

        BeamMesh.LookAt(BeamMesh.GlobalPosition + GlobalBasis * direction);
        BeamMesh.RotateObjectLocal(Vector3.Right, Mathf.Pi * 0.5f);

        BeamMesh.Scale = Vector3.One;
    }
}