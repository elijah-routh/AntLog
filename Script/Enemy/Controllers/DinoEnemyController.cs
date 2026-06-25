using Godot;

namespace Game.Enemy
{
    public partial class DinoEnemyController : EnemyControllerBase, IGrabStateReceiver
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

        [ExportGroup("Wander")]
        [Export] public float WanderRadius { get; set; } = 12f;
        [Export] public float WanderPointReachedDistance { get; set; } = 1.25f;
        [Export] public float WanderPauseMin { get; set; } = 0.4f;
        [Export] public float WanderPauseMax { get; set; } = 1.2f;
        [Export] public float WanderChargeChance { get; set; } = 0.15f;

        [ExportGroup("Orbit")]
        [Export] public float OrbitEnterDistance { get; set; } = 16f;
        [Export] public float OrbitIdealDistance { get; set; } = 8f;
        [Export] public float OrbitDistanceTolerance { get; set; } = 1.5f;
        [Export] public float OrbitSpeedMultiplier { get; set; } = 0.85f;
        [Export] public float OrbitDirectionChangeChance { get; set; } = 0.15f;

        [ExportGroup("Orbit Spacing")]
        [Export] public float BackUpDistance { get; set; } = 6.5f;
        [Export] public float BackUpStrength { get; set; } = 5.0f;
        [Export] public float OrbitSideOffset { get; set; } = 4.0f;

        [ExportGroup("Charge Timing")]
        [Export] public float ChargeAttemptCooldown { get; set; } = 2.0f;

        [ExportGroup("Charge Intent")]
        [Export] public float ChaseChargeMinDelay { get; set; } = 1.0f;
        [Export] public float ChaseChargeMaxDelay { get; set; } = 3.0f;
        [Export] public float ChargeMinDistance { get; set; } = 5.5f;
        [Export] public float ChargeMaxDistance { get; set; } = 11f;

        [ExportGroup("Attack Hitboxes")]
        [Export] public DamageHitboxComponent ChargeHitbox { get; set; }

        [ExportGroup("Feedback")]
        [Export] public AttackFeedbackComponent AttackFeedback;

        public bool CanCharge { get; private set; } = true;

        private float _chargeCooldownTimer;
        private int _chargesUsed;

        public bool IsGrabbed { get; private set; }
        public bool IsThrown { get; private set; }

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

            if (IsGrabbed || IsThrown)
                return;

            if (IsInState<DinoChargeState>())
                return;

            ChangeState(new DinoChaseOrbitState(this, Enemy, Target));
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

        public void SetGrabbed(bool grabbed)
        {
            IsGrabbed = grabbed;

            if (grabbed)
            {
                ChangeState(new DinoGrabState(this, Enemy));
            }
            else
            {
                if (IsInState<DinoDeadState>())
                    return;

                ChangeState(new DinoTiredState(this, Enemy, Target));
            }
        }

        public void OnGrabbed()
        {
            GD.Print($"{Enemy.Name}: Dino controller received OnGrabbed");

            if (IsInState<DinoDeadState>())
                return;

            IsGrabbed = true;
            IsThrown = false;

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            ChangeState(new DinoGrabState(this, Enemy));
        }

        public void OnReleased()
        {
            GD.Print($"{Enemy.Name}: Dino controller received OnReleased");

            if (IsInState<DinoDeadState>())
                return;

            IsGrabbed = false;
            IsThrown = false;

            ChangeState(new DinoTiredState(this, Enemy, Target));
        }

        public void OnThrown()
        {
            GD.Print($"{Enemy.Name}: Dino controller received OnThrown");

            if (IsInState<DinoDeadState>())
                return;

            IsGrabbed = false;
            IsThrown = true;

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            ChangeState(new DinoThrownState(this, Enemy));
        }

        public void OnThrowFinished()
        {
            GD.Print($"{Enemy.Name}: Dino controller received OnThrowFinished");

            if (IsInState<DinoDeadState>())
                return;

            IsGrabbed = false;
            IsThrown = false;

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            ChangeState(new DinoTiredState(this, Enemy, Target));
        }

        public bool ShouldEnterOrbit()
        {
            return HasValidTarget() && GetDistanceToTarget() <= OrbitEnterDistance;
        }

        public bool IsInGoodChargeRange()
        {
            float distance = GetDistanceToTarget();

            return distance >= ChargeMinDistance &&
                   distance <= ChargeMaxDistance;
        }

        public bool CanAttemptCharge()
        {
            return CanCharge &&
                   HasValidTarget() &&
                   IsInGoodChargeRange();
        }

        public float GetRandomChaseChargeDelay()
        {
            return (float)GD.RandRange(ChaseChargeMinDelay, ChaseChargeMaxDelay);
        }

        public bool RollWanderChargeChance()
        {
            return GD.Randf() <= WanderChargeChance;
        }
    }
}