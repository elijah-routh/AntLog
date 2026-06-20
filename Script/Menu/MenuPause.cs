using Godot;

public partial class MenuPause : CanvasLayer
{
    [Export] private AnimationPlayer animationPlayer;
    [Export] private Button resumeButton;
    [Export] private Button quitButton;

    public override void _Ready()
    {
        resumeButton.Pressed += ResumeGame;
        quitButton.Pressed += QuitToTitle;

        SetPaused(false);
    }

    public override void _Process(double delta)
    {
        if (PlayerInput.PausePressed)
            SetPaused(!GetTree().Paused);
    }

    private void SetPaused(bool paused)
    {
        GetTree().Paused = paused;
        Visible = paused;

        Input.MouseMode = paused
            ? Input.MouseModeEnum.Visible
            : Input.MouseModeEnum.Captured;

        if (paused)
        {
            resumeButton.GrabFocus();
            animationPlayer.Play("blur");
        }
        else
        {
            animationPlayer.PlayBackwards("blur");
        }
    }

    private void ResumeGame()
    {
        SetPaused(false);
    }

    private void QuitToTitle()
    {
        SetPaused(false);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile("res://Scene/Menu/menu_title.tscn");
    }
}