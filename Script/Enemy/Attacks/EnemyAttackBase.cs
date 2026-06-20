using Godot;

namespace Game.Enemy
{
    public abstract class EnemyAttackBase
    {
        public bool IsRunning { get; protected set; }
        public float Cooldown { get; protected set; } = 2f;

        private float _cooldownTimer = 0f;

        public bool CanUse => !IsRunning && _cooldownTimer <= 0f;

        public virtual void PhysicsUpdate(double delta)
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= (float)delta;
        }

        public bool TryStart()
        {
            if (!CanUse)
                return false;

            IsRunning = true;
            return true;
        }

        protected void Finish()
        {
            IsRunning = false;
            _cooldownTimer = Cooldown;
        }
    }
}