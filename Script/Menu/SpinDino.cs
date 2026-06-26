using Godot;

public partial class SpinDino : Node3D
{
    [Export]
    public float SpinSpeed = 45.0f;

    public override void _Process(double delta)
    {
        RotationDegrees += new Vector3(0, SpinSpeed * (float)delta, 0);
    }
}