using Godot;

namespace Game.Enemy
{
    public partial class DinoEnemyController : EnemyControllerBase
    {
        [ExportGroup("References")]
        [Export] public DinoAnimationController Animations { get; set; }

        [ExportGroup("Pathfinding")]
        [Export] public float PathUpdateInterval { get; set; } = 0.2f;

        [ExportGroup("Detection")]
        [Export] public float DetectionRange { get; set; } = 22f;
        [Export] public float LoseTargetRange { get; set; } = 34f;

        [ExportGroup("Rotation")]
        [Export] public float RotationSpeed { get; set; } = 8f;
        [Export] public float RunRotationSpeed { get; set; } = 8f;
        [Export] public float TiredRotationSpeed { get; set; } = 0.75f;

        [ExportGroup("Run")]
        [Export] public float ChargeStartDistance { get; set; } = 5f;

        [ExportGroup("Charge Hop")]
        [Export] public float ChargeHopDuration { get; set; } = 0.45f;
        [Export] public float ChargeHopSideSpeedMultiplier { get; set; } = 0.65f;
        [Export(PropertyHint.Range, "0,1,0.05")]
        public float HopInPlaceChance { get; set; } = 0.25f;

        [ExportGroup("Charge")]
        [Export] public float ChargeSpeedMultiplier { get; set; } = 1.8f;
        [Export] public float ChargeMinDuration { get; set; } = 0.9f;
        [Export] public float ChargeMaxDuration { get; set; } = 1.8f;
        [Export] public float ChargeCooldown { get; set; } = 0.35f;
        [Export] public int ChargesBeforeTired { get; set; } = 3;

        [ExportGroup("Tired")]
        [Export] public float TiredDuration { get; set; } = 2.0f;

        public bool CanCharge { get; private set; } = true;

        private float _chargeCooldownTimer;
        private int _chargesUsed;

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            UpdateChargeCooldown((float)delta);
        }

        protected override void EnterInitialState()
        {
            ResetChargeChain();
            ChangeState(new DinoIdleState(this, Enemy));
        }

        protected override void OnDamaged(float damage)
        {
            if (Target == null)
                return;

            if (IsInState<DinoDeadState>())
                return;

            if (IsInState<DinoChargeState>())
                return;

            ChangeState(new DinoRunState(this, Enemy, Target));
        }

        protected override void OnDied()
        {
            base.OnDied();

            ChangeState(new DinoDeadState(this, Enemy));
        }

        private void UpdateChargeCooldown(float delta)
        {
            if (CanCharge)
                return;

            _chargeCooldownTimer -= delta;

            if (_chargeCooldownTimer <= 0f)
            {
                _chargeCooldownTimer = 0f;
                CanCharge = true;
            }
        }

        public void StartChargeCooldown()
        {
            CanCharge = false;
            _chargeCooldownTimer = ChargeCooldown;
        }

        public void RegisterChargeUsed()
        {
            _chargesUsed++;
        }

        public bool HasUsedAllCharges()
        {
            return _chargesUsed >= ChargesBeforeTired;
        }

        public void ResetChargeChain()
        {
            _chargesUsed = 0;
        }

        public float GetRandomChargeDuration()
        {
            return (float)GD.RandRange(ChargeMinDuration, ChargeMaxDuration);
        }

        public float GetDistanceToTarget()
        {
            if (Enemy == null || Target == null)
                return float.MaxValue;

            return Enemy.GlobalPosition.DistanceTo(Target.GlobalPosition);
        }

        public bool HasValidTarget()
        {
            return Target != null;
        }

        public bool IsTargetInDetectionRange()
        {
            return GetDistanceToTarget() <= DetectionRange;
        }

        public bool IsTargetLost()
        {
            return GetDistanceToTarget() > LoseTargetRange;
        }

        public bool ShouldStartCharge()
        {
            return CanCharge && GetDistanceToTarget() <= ChargeStartDistance;
        }

        public Vector3 GetDirectionToTarget()
        {
            if (Enemy == null || Target == null)
                return Vector3.Zero;

            Vector3 direction = Target.GlobalPosition - Enemy.GlobalPosition;
            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.001f)
                return Vector3.Zero;

            return direction.Normalized();
        }

        public Vector3 GetForwardDirection()
        {
            if (Enemy == null)
                return Vector3.Zero;

            Vector3 forward = -Enemy.GlobalTransform.Basis.Z;
            forward.Y = 0f;

            if (forward.LengthSquared() <= 0.001f)
                return Vector3.Zero;

            return forward.Normalized();
        }

        public void FaceTarget(double delta)
        {
            FaceTarget(delta, RotationSpeed);
        }

        public void FaceTarget(double delta, float rotationSpeed)
        {
            if (Enemy == null || Target == null)
                return;

            Vector3 direction = GetDirectionToTarget();

            if (direction == Vector3.Zero)
                return;

            FaceDirection(direction, delta, rotationSpeed);
        }

        public void FaceDirection(Vector3 direction, double delta, float rotationSpeed)
        {
            if (Enemy == null)
                return;

            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.001f)
                return;

            direction = direction.Normalized();

            float targetYaw = Mathf.Atan2(direction.X, direction.Z);

            Vector3 rotation = Enemy.GlobalRotation;

            rotation.Y = Mathf.LerpAngle(
                rotation.Y,
                targetYaw,
                Mathf.Clamp(rotationSpeed * (float)delta, 0f, 1f)
            );

            Enemy.GlobalRotation = rotation;
        }
    }
}