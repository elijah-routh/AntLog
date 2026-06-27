using Godot;

public partial class ScoreManager : Node
{
    [Signal]
    public delegate void ScoreChangedEventHandler(int score);

    [Signal]
    public delegate void PointsAddedEventHandler(int amount);

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

        EmitSignal(SignalName.ScoreChanged, Score);
        EmitSignal(SignalName.PointsAdded, amount);

        GD.Print($"Score: {Score}");
    }

    public void ResetScore()
    {
        Score = 0;
        EmitSignal(SignalName.ScoreChanged, Score);
    }
}