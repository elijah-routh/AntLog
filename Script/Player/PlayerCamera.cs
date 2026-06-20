using Godot;

public partial class PlayerCamera : Node3D
{

    [Export] public Node3D Target;

    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float ControllerSensitivity = 0.05f;
    [Export] public float MinPitch = -60f;
    [Export] public float MaxPitch = 45f;
    [Export] public Vector3 Offset = new Vector3(0, 1.5f, 0);

    [Export] public SpringArm3D SpringArm;
    [Export] public float MinSpringLength = 1.0f;
    //[Export] public float MaxSpringLength = 4.0f;

    private Vector2 _cameraInput = Vector2.Zero;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        bool isCameraMotion =
            @event is InputEventMouseMotion &&
            Input.MouseMode == Input.MouseModeEnum.Captured;

        if (!isCameraMotion)
            return;

        InputEventMouseMotion mouse =
            @event as InputEventMouseMotion;

        _cameraInput =
            mouse.ScreenRelative * MouseSensitivity;
    }

    public override void _PhysicsProcess(double delta)
    {

        if (Target != null)
        {
            GlobalPosition = Target.GlobalPosition + Offset;
        }

        // Right stick input
        Vector2 controllerLook =
            PlayerInput.CameraLook * ControllerSensitivity;

        // Combine controller + mouse
        Vector2 lookInput = controllerLook;

        if (_cameraInput != Vector2.Zero)
        {
            lookInput += _cameraInput;
        }

        // Rotate camera
        Rotation += new Vector3(
            -lookInput.Y,
            -lookInput.X,
            0
        );

        // Clamp vertical rotation
        Rotation = new Vector3(
            Mathf.Clamp(
                Rotation.X,
                Mathf.DegToRad(MinPitch),
                Mathf.DegToRad(MaxPitch)
            ),
            Rotation.Y,
            0
        );

        // Clear mouse delta
        _cameraInput = Vector2.Zero;

        //min camera zoom
        if (SpringArm != null)
        {
            SpringArm.SpringLength = Mathf.Max(
                SpringArm.SpringLength,
                MinSpringLength
            );
        }
    }
}
