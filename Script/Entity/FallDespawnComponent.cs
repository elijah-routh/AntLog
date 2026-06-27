using Godot;

namespace Game.Components
{
    public partial class FallDespawnComponent : Node
    {
        [Export] public Node3D Target;
        [Export] public float DespawnY = -50f;

        public override void _Ready()
        {
            if (Target == null)
                Target = GetParent<Node3D>();
        }

        public override void _Process(double delta)
        {
            if (Target == null)
                return;

            if (Target.GlobalPosition.Y <= DespawnY)
            {
                Target.QueueFree();
            }
        }
    }
}