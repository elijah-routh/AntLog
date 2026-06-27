using Godot;
using Game.UI;

public partial class HUD : CanvasLayer
{
    [ExportGroup("References")]
    [Export] public ScoreManager ScoreManager;
    [Export] public PackedScene ScorePopupScene;
    [Export] public Control PopupRoot;

    [ExportGroup("Popup Settings")]
    [Export] public float ScreenPadding = 80f;

    private Label _scoreLabel;
    private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();

        _scoreLabel = GetNodeOrNull<Label>("ScoreCount");

        if (_scoreLabel == null)
            GD.PushWarning("HUD: ScoreCount label was not found.");

        if (PopupRoot == null)
            GD.PushWarning("HUD: PopupRoot is not assigned.");

        if (ScorePopupScene == null)
            GD.PushWarning("HUD: ScorePopupScene is not assigned.");

        if (ScoreManager == null)
        {
            ScoreManager = GetTree().GetFirstNodeInGroup("score_manager") as ScoreManager;
        }

        if (ScoreManager == null)
        {
            GD.PushWarning("HUD: ScoreManager is not assigned and could not be found in group 'score_manager'.");
            return;
        }

        ScoreManager.ScoreChanged += OnScoreChanged;
        ScoreManager.PointsAdded += OnPointsAdded;

        OnScoreChanged(ScoreManager.Score);
    }

    public override void _ExitTree()
    {
        if (ScoreManager == null)
            return;

        ScoreManager.ScoreChanged -= OnScoreChanged;
        ScoreManager.PointsAdded -= OnPointsAdded;
    }

    private void OnScoreChanged(int score)
    {
        if (_scoreLabel == null)
            return;

        _scoreLabel.Text = $"Score: {score}";
    }

    private void OnPointsAdded(int amount)
    {
        ShowScorePopup(amount);
    }

    private void ShowScorePopup(int amount)
    {
        if (ScorePopupScene == null || PopupRoot == null)
            return;

        ScorePopup popup = ScorePopupScene.Instantiate<ScorePopup>();
        PopupRoot.AddChild(popup);

        Vector2 viewportSize = PopupRoot.GetViewportRect().Size;

        float x = _rng.RandfRange(ScreenPadding, viewportSize.X - ScreenPadding);
        float y = _rng.RandfRange(ScreenPadding, viewportSize.Y - ScreenPadding);

        popup.Position = new Vector2(x, y);
        popup.Play(amount);
    }
}