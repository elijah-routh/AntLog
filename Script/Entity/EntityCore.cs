using Godot;

namespace Game.Entity
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }

    public interface IHealable
    {
        void Heal(float amount);
    }

    public interface IKillable
    {
        void Kill();
    }

    public interface IMovable
    {
        void Move(Vector3 direction);
        void Stop();
    }

    public interface IKnockable
    {
        void ApplyKnockback(Vector3 force);
    }

}