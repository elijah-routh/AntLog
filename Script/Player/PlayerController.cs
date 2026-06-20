using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export] public Node3D CameraPivot;
    [Export] public PlayerMoveComponent Movement;
    [Export] public GroundAlignComponent GroundAlignment;

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        Movement.ApplyGravity(this, dt);
        Movement.HandleJump(this);
        Movement.HandleMovement(this, CameraPivot, dt);

        MoveAndSlide();

        GroundAlignment.AlignToGround(this, dt);
    }
}