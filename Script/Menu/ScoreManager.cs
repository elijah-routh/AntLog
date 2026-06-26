using Godot;

public partial class ScoreManager : Node
{
    [Signal]
    public delegate void ScoreChangedEventHandler(int score);

    public int Score { get; private set; }

    public override void _Ready()
    {
        AddToGroup("score_manager");
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        Score += amount;
        EmitSignal(nameof(ScoreChanged), Score);

        GD.Print($"Score: {Score}");
    }

    public void ResetScore()
    {
        Score = 0;
        EmitSignal(nameof(ScoreChanged), Score);
    }
}
