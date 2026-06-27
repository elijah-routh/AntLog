using Godot;

public partial class GameOverController : Node
{
    [Export] private PlayerController player;

    [ExportGroup("Death Camera")]
    [Export] private ArenaPreviewCamera deathCameraRig;
    [Export] private Camera3D playerCamera;

    [ExportGroup("UI")]
    [Export] private MenuPause pauseMenu;
    [Export] private MenuGameOver gameOverMenu;

    [ExportGroup("Stats Source")]
    [Export] private ScoreManager scoreManager;
    [Export] private int kills;
    [Export] private float survivalTime;

    private bool _gameOver;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        if (player != null)
            player.PlayerDied += TriggerGameOver;

        if (scoreManager == null)
            scoreManager = GetTree().GetFirstNodeInGroup("score_manager") as ScoreManager;

        if (playerCamera != null)
            playerCamera.Current = true;

        if (deathCameraRig != null)
        {
            deathCameraRig.Visible = false;
            deathCameraRig.ProcessMode = ProcessModeEnum.Always;

            if (deathCameraRig.Camera != null)
                deathCameraRig.Camera.Current = false;
        }
    }

    public override void _Process(double delta)
    {
        if (_gameOver)
            return;

        survivalTime += (float)delta;
    }

    public void TriggerGameOver()
    {
        if (_gameOver)
            return;

        _gameOver = true;

        Vector3 deathPosition = player != null
            ? player.GlobalPosition
            : Vector3.Zero;

        if (pauseMenu != null)
            pauseMenu.Visible = false;

        if (deathCameraRig != null)
        {
            deathCameraRig.Visible = true;
            deathCameraRig.GlobalPosition = deathPosition;
            deathCameraRig.LookAtTargetMode = true;
            deathCameraRig.SetTarget(player);

            if (deathCameraRig.Camera != null)
                deathCameraRig.Camera.Current = true;
        }

        if (playerCamera != null)
            playerCamera.Current = false;

        if (gameOverMenu != null)
        {
            int finalScore = scoreManager != null ? scoreManager.Score : 0;
            gameOverMenu.ShowGameOver(finalScore, kills, survivalTime);
        }

        Input.MouseMode = Input.MouseModeEnum.Visible;

        // Do not pause. Let the world keep running behind the menu.
        // GetTree().Paused = true;
    }

    public void SetKills(int value)
    {
        kills = value;
    }

    public void AddKill()
    {
        kills++;
    }

}