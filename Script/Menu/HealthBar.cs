using Godot;

public partial class HealthBar : ProgressBar
{
    private Timer _timer;
    private ProgressBar _damageBar;

    private double _health = 0;

    public double Health
    {
        get => _health;
        set => SetHealth(value);
    }

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _damageBar = GetNode<ProgressBar>("DamageBar");

        _timer.Timeout += OnTimerTimeout;
    }

    private void SetHealth(double newHealth)
    {
        double prevHealth = _health;

        _health = Mathf.Min(MaxValue, newHealth);
        Value = _health;

        if (_health <= 0)
        {
            QueueFree();
        }

        if (_health < prevHealth)
        {
            _timer.Start();
        }
        else
        {
            _damageBar.Value = _health;
        }
    }

    private void OnTimerTimeout()
    {
        _damageBar.Value = _health;
    }

    public void InitHealth(double health)
    {
        _health = health;

        MaxValue = _health;
        Value = _health;

        _damageBar.MaxValue = _health;
        _damageBar.Value = _health;
    }
}