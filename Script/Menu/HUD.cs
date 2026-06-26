using Godot;

public partial class HUD : CanvasLayer
{
    private Label _starLabel;
    private Label _scoreLabel;

    private PlayerInventory _inventory;
    private ScoreManager _scoreManager;

    public override void _Ready()
    {
        _scoreLabel = GetNodeOrNull<Label>("ScoreCount");

        _scoreManager = GetTree()
            .GetFirstNodeInGroup("score_manager") as ScoreManager;

        if (_scoreManager != null && _scoreLabel != null)
        {
            _scoreManager.ScoreChanged += OnScoreChanged;
            OnScoreChanged(_scoreManager.Score);
        }
    }

    private void OnScoreChanged(int score)
    {
        _scoreLabel.Text = score.ToString();
    }
}