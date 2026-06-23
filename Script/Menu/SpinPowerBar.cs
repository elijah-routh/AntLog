using Godot;

public partial class SpinPowerBar : ProgressBar
{
    [Export] public SpinPowerComponent SpinPower;
    [Export] public int MaxPowerStep = 4;
    [Export] public bool PrintDebug = true;

    public override void _Ready()
    {
        MinValue = 0;
        MaxValue = MaxPowerStep;
        Step = 1;
        Value = 0;

        if (SpinPower == null)
        {
            GD.PrintErr($"{Name}: SpinPower is NOT assigned.");
        }
        else
        {
            GD.Print($"{Name}: SpinPower assigned to {SpinPower.Name}");
        }
    }

    public override void _Process(double delta)
    {
        if (SpinPower == null)
            return;

        int step = SpinPower.CurrentPowerStep;

        Value = step;

        if (PrintDebug)
        {
            GD.Print($"{Name}: Spin Step = {step}, Bar Value = {Value}");
        }
    }
}