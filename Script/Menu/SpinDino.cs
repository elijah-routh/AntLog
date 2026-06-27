using Godot;

public partial class SpinDino : Node3D
{
    [ExportGroup("Spin Settings")]
    [Export] public float SpinSpeedDegrees = 45.0f;

    [ExportGroup("Spin Axis")]
    [Export] public bool SpinX = false;
    [Export] public bool SpinY = true;
    [Export] public bool SpinZ = false;

    public override void _Process(double delta)
    {
        Vector3 axis = Vector3.Zero;

        if (SpinX)
            axis.X = 1.0f;

        if (SpinY)
            axis.Y = 1.0f;

        if (SpinZ)
            axis.Z = 1.0f;

        if (axis == Vector3.Zero)
            return;

        axis = axis.Normalized();

        Rotate(axis, Mathf.DegToRad(SpinSpeedDegrees) * (float)delta);
    }
}