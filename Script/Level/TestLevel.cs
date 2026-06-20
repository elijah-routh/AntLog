using Godot;

public partial class TestLevel : Node3D
{
    public override void _Ready()
    {
        SoundManager.Instance.PlayLevelMusic();
    }
}