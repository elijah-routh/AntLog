using Godot;

namespace Game.Targets
{
    public partial class RailRider3D : Node
    {
        public enum MotionMode
        {
            Loop,
            PingPong
        }

        [ExportGroup("References")]
        [Export] public Node3D MoveRoot;
        [Export] public Rail3D Rail;

        [ExportGroup("Motion")]
        [Export] public float Speed = 0.25f;
        [Export] public MotionMode Mode = MotionMode.Loop;
        [Export] public bool RandomizeStartingProgress = true;
        [Export] public bool MoveOnReady = true;

        public float Progress { get; private set; }

        private int _direction = 1;
        private bool _isMoving;

        public override void _Ready()
        {
            if (MoveRoot == null)
                MoveRoot = GetParent<Node3D>();

            if (RandomizeStartingProgress)
                Progress = GD.Randf();

            _isMoving = MoveOnReady;

            UpdatePosition();
        }

        public override void _Process(double delta)
        {
            if (!_isMoving || Rail == null || MoveRoot == null)
                return;

            float amount = Speed * (float)delta * _direction;

            Progress += amount;

            if (Mode == MotionMode.Loop)
            {
                Progress = Wrap01(Progress);
            }
            else
            {
                if (Progress >= 1.0f)
                {
                    Progress = 1.0f;
                    _direction = -1;
                }
                else if (Progress <= 0.0f)
                {
                    Progress = 0.0f;
                    _direction = 1;
                }
            }

            UpdatePosition();
        }

        public void Setup(Rail3D rail, float speed, MotionMode mode, float startingProgress = -1.0f)
        {
            Rail = rail;
            Speed = speed;
            Mode = mode;

            Progress = startingProgress >= 0.0f
                ? Mathf.Clamp(startingProgress, 0.0f, 1.0f)
                : GD.Randf();

            _direction = GD.Randf() > 0.5f ? 1 : -1;
            _isMoving = true;

            UpdatePosition();
        }

        public void Stop()
        {
            _isMoving = false;
        }

        public void Resume()
        {
            _isMoving = true;
        }

        private void UpdatePosition()
        {
            if (Rail == null || MoveRoot == null)
                return;

            MoveRoot.GlobalPosition = Rail.GetGlobalPoint(Progress);
        }

        private float Wrap01(float value)
        {
            value %= 1.0f;

            if (value < 0.0f)
                value += 1.0f;

            return value;
        }
    }
}