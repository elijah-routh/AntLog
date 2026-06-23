using Godot;

public partial class SpinPowerComponent : Node
{
    [ExportGroup("Spin Power")]
    [Export] public float MaxSpinPower = 1.0f;
    [Export] public float SpinBuildRate = 0.35f;
    [Export] public float SpinDecayRate = 0.6f;

    [ExportGroup("Step Thresholds")]
    [Export] public float Step1Threshold = 0.25f;
    [Export] public float Step2Threshold = 0.50f;
    [Export] public float Step3Threshold = 0.75f;
    [Export] public float Step4Threshold = 1.00f;

    [ExportGroup("Force Multipliers")]
    [Export] public float Step0ForceMultiplier = 1.0f;
    [Export] public float Step1ForceMultiplier = 1.25f;
    [Export] public float Step2ForceMultiplier = 1.6f;
    [Export] public float Step3ForceMultiplier = 2.0f;
    [Export] public float Step4ForceMultiplier = 2.5f;

    [ExportGroup("Damage Multipliers")]
    [Export] public float Step0DamageMultiplier = 1.0f;
    [Export] public float Step1DamageMultiplier = 1.25f;
    [Export] public float Step2DamageMultiplier = 1.6f;
    [Export] public float Step3DamageMultiplier = 2.0f;
    [Export] public float Step4DamageMultiplier = 2.5f;

    [ExportGroup("Debug")]
    [Export] public bool PrintStepChanges = true;

    public float CurrentSpinPower { get; private set; }
    public float SpinPowerPercent => MaxSpinPower <= 0.0f ? 0.0f : CurrentSpinPower / MaxSpinPower;
    public int CurrentPowerStep { get; private set; }

    private bool _isHoldingThrowable;
    private int _previousStep;

    public override void _PhysicsProcess(double delta)
    {
        UpdatePowerStep();

        if (PrintStepChanges && CurrentPowerStep != _previousStep)
        {
            GD.Print($"Spin Power Step: {CurrentPowerStep}");
            _previousStep = CurrentPowerStep;
        }
    }

    public void SetHoldingThrowable(bool isHoldingThrowable)
    {
        _isHoldingThrowable = isHoldingThrowable;

        if (!_isHoldingThrowable)
        {
            ResetPower();
        }
    }

    public void BuildPower(float delta, float spinSpeedPercent)
    {
        if (!_isHoldingThrowable)
            return;

        spinSpeedPercent = Mathf.Clamp(spinSpeedPercent, 0.0f, 1.0f);

        float buildAmount = SpinBuildRate * spinSpeedPercent * delta;
        CurrentSpinPower = Mathf.Min(CurrentSpinPower + buildAmount, MaxSpinPower);

        UpdatePowerStep();
    }

    public void DecayPower(float delta)
    {
        if (CurrentSpinPower <= 0.0f)
            return;

        CurrentSpinPower = Mathf.Max(CurrentSpinPower - SpinDecayRate * delta, 0.0f);

        UpdatePowerStep();
    }

    public void ResetPower()
    {
        CurrentSpinPower = 0.0f;
        CurrentPowerStep = 0;
        _previousStep = 0;
    }

    public SpinThrowModifier GetThrowModifier()
    {
        return CurrentPowerStep switch
        {
            1 => new SpinThrowModifier(1, Step1ForceMultiplier, Step1DamageMultiplier),
            2 => new SpinThrowModifier(2, Step2ForceMultiplier, Step2DamageMultiplier),
            3 => new SpinThrowModifier(3, Step3ForceMultiplier, Step3DamageMultiplier),
            4 => new SpinThrowModifier(4, Step4ForceMultiplier, Step4DamageMultiplier),
            _ => new SpinThrowModifier(0, Step0ForceMultiplier, Step0DamageMultiplier),
        };
    }

    private void UpdatePowerStep()
    {
        float percent = SpinPowerPercent;

        if (percent >= Step4Threshold)
        {
            CurrentPowerStep = 4;
        }
        else if (percent >= Step3Threshold)
        {
            CurrentPowerStep = 3;
        }
        else if (percent >= Step2Threshold)
        {
            CurrentPowerStep = 2;
        }
        else if (percent >= Step1Threshold)
        {
            CurrentPowerStep = 1;
        }
        else
        {
            CurrentPowerStep = 0;
        }
    }
}