using Godot;

public partial class HUD : CanvasLayer
{
    private Label _starLabel;
    private Label _scoreLabel;

    private PlayerInventory _inventory;
    [Export] public ScoreManager ScoreManager;

    public override void _Ready()
    {
        _scoreLabel = GetNodeOrNull<Label>("ScoreCount");

        if (ScoreManager != null && _scoreLabel != null)
        {
            ScoreManager.ScoreChanged += OnScoreChanged;
            OnScoreChanged(ScoreManager.Score);
        }
    }

    private void OnScoreChanged(int score)
    {
        _scoreLabel.Text = score.ToString();
    }
}