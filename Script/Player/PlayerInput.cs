using Godot;

public static class PlayerInput
{
    public static Vector2 Movement =>
        Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_back"
        );

    public static Vector2 CameraLook => 
        Input.GetVector(
            "camera_left",
            "camera_right",
            "camera_up",
            "camera_down"
        );

    public static bool JumpPressed =>
        Input.IsActionJustPressed("jump");

    public static bool PausePressed =>
        Input.IsActionJustPressed("pause");

    public static bool ShootPressed =>
        Input.IsActionPressed("shoot");

    public static bool BoostPressed =>
        Input.IsActionPressed("boost");

    public static bool DamagePressed =>
        Input.IsActionJustPressed("debug_damage");
}
