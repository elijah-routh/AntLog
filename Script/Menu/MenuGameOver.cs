using Godot;

public partial class MenuGameOver : CanvasLayer
{
    [Export] private AnimationPlayer animationPlayer;

    [Export] private Button retryButton;
    [Export] private Button quitButton;

    [ExportGroup("Stats")]
    [Export] private Label scoreLabel;
    [Export] private Label killsLabel;
    [Export] private Label timeLabel;

    private string _gameScenePath = "res://Levels/IslandMap.tscn";
    private string _mainMenuScenePath = "res://Scene/Menu/MainMenu.tscn";

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        retryButton.ProcessMode = ProcessModeEnum.Always;
        quitButton.ProcessMode = ProcessModeEnum.Always;

        retryButton.Pressed += RetryGame;
        quitButton.Pressed += QuitToTitle;

        Visible = false;
    }

    public void ShowGameOver(int score, int kills, float survivalTime)
    {
        Visible = true;

        Input.MouseMode = Input.MouseModeEnum.Visible;

        if (scoreLabel != null)
            scoreLabel.Text = $"Score: {score}";

        if (killsLabel != null)
            killsLabel.Text = $"Kills: {kills}";

        if (timeLabel != null)
            timeLabel.Text = $"Time: {FormatTime(survivalTime)}";

        retryButton.GrabFocus();

        if (animationPlayer != null)
            animationPlayer.Play("blur");
    }

    private void RetryGame()
    {
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().ChangeSceneToFile(_gameScenePath);
    }

    private void QuitToTitle()
    {
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().ChangeSceneToFile(_mainMenuScenePath);
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }
}