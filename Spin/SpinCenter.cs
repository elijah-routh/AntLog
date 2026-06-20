using Godot;

public partial class SpinCenter : Node3D
{
    [ExportGroup("Input")]
    [Export] public string SpinAction = "spin";

    [ExportGroup("Spin")]
    [Export] public Vector3 SpinAxis = Vector3.Up;
    [Export] public float SpinSpeed = 180.0f;

    private bool _isSpinning = false;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(SpinAction))
        {
            _isSpinning = !_isSpinning;
        }

        if (!_isSpinning)
            return;

        float radians = Mathf.DegToRad(SpinSpeed) * (float)delta;

        Rotate(SpinAxis.Normalized(), radians);
    }
}