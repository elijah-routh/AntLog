using Godot;
using Game.Entity;

namespace Game.Components
{
    public partial class HurtboxComponent : Area3D
    {
        [Export] public Node DamageableRoot;
        [Export] public bool CanReceiveDamage = true;

        private IDamageable _damageable;

        public override void _Ready()
        {
            Monitoring = true;
            Monitorable = true;

            if (DamageableRoot == null)
                DamageableRoot = Owner;

            _damageable = FindDamageable(DamageableRoot);

            if (_damageable == null)
                GD.PushWarning($"{Name}: No IDamageable found for hurtbox.");
        }

        public void TakeDamage(float damage)
        {
            if (!CanReceiveDamage)
                return;

            _damageable?.TakeDamage(damage);
        }

        private IDamageable FindDamageable(Node root)
        {
            if (root == null)
                return null;

            if (root is IDamageable damageable)
                return damageable;

            foreach (Node child in root.GetChildren())
            {
                if (child is IDamageable childDamageable)
                    return childDamageable;
            }

            return null;
        }
    }
}