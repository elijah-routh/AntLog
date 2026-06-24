using Godot;
using Game.Entity;

namespace Game.Enemy
{
    public partial class DamageHitboxComponent : Area3D
    {
        [ExportGroup("Damage")]
        [Export] public float Damage = 10f;
        [Export] public bool DisableAfterHit = false;

        [ExportGroup("Targeting")]
        [Export] public bool OnlyTargetPlayer = true;
        [Export] public bool ExcludePlayer = false;
        [Export] public string PlayerGroupName = "Player";

        [ExportGroup("Hit Rules")]
        [Export] public bool HitOncePerActivation = true;
        [Export] public float HitCooldown = 0.25f;

        [ExportGroup("Knockback")]
        [Export] public bool ApplyKnockback = false;
        [Export] public float KnockbackForce = 12f;
        [Export] public float UpwardKnockback = 1.5f;
        [Export] public Node3D KnockbackSource;

        private readonly Godot.Collections.Array<Node> _hitTargets = new();
        private readonly Godot.Collections.Dictionary<Node, float> _hitCooldowns = new();

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;

            Monitoring = false;
            Monitorable = false;
        }

        public override void _PhysicsProcess(double delta)
        {
            UpdateHitCooldowns((float)delta);
        }

        public void EnableHitbox()
        {
            _hitTargets.Clear();
            _hitCooldowns.Clear();

            Monitoring = true;
            Monitorable = true;
        }

        public void DisableHitbox()
        {
            Monitoring = false;
            Monitorable = false;

            _hitTargets.Clear();
            _hitCooldowns.Clear();
        }

        public void SetDamage(float damage)
        {
            Damage = damage;
        }

        private void OnBodyEntered(Node3D body)
        {
            GD.Print($"[DamageHitbox] Body entered: {body.Name}");

            if (!Monitoring || body == null)
            {
                GD.Print("[DamageHitbox] Ignored: not monitoring or body null.");
                return;
            }

            if (!IsValidTarget(body))
            {
                GD.Print($"[DamageHitbox] Ignored: invalid target {body.Name}");
                return;
            }

            if (HitOncePerActivation && _hitTargets.Contains(body))
            {
                GD.Print($"[DamageHitbox] Ignored: already hit {body.Name}");
                return;
            }

            if (!HitOncePerActivation && _hitCooldowns.ContainsKey(body))
            {
                GD.Print($"[DamageHitbox] Ignored: cooldown active for {body.Name}");
                return;
            }

            IDamageable damageable = FindDamageable(body);

            if (damageable == null)
            {
                GD.Print($"[DamageHitbox] No IDamageable found on {body.Name}");
                return;
            }

            GD.Print($"[DamageHitbox] Damaging {body.Name} for {Damage}");

            damageable.TakeDamage(Damage);

            if (ApplyKnockback)
            {
                GD.Print($"[DamageHitbox] Applying knockback to {body.Name}");
                ApplyKnockbackTo(body);
            }

            RegisterHit(body);

            if (DisableAfterHit)
                DisableHitbox();
        }

        private void RegisterHit(Node body)
        {
            if (HitOncePerActivation)
            {
                if (!_hitTargets.Contains(body))
                    _hitTargets.Add(body);

                return;
            }

            _hitCooldowns[body] = HitCooldown;
        }

        private void UpdateHitCooldowns(float delta)
        {
            if (_hitCooldowns.Count == 0)
                return;

            Godot.Collections.Array<Node> expiredTargets = new();

            foreach (Node target in _hitCooldowns.Keys)
            {
                _hitCooldowns[target] -= delta;

                if (_hitCooldowns[target] <= 0f)
                    expiredTargets.Add(target);
            }

            foreach (Node target in expiredTargets)
            {
                _hitCooldowns.Remove(target);
            }
        }

        private bool IsValidTarget(Node body)
        {
            bool isPlayer = IsPlayer(body);

            // Highest priority:
            // this hitbox only damages objects marked as Player.
            if (OnlyTargetPlayer)
                return isPlayer;

            // Second priority:
            // this hitbox damages anything except objects marked as Player.
            if (ExcludePlayer)
                return !isPlayer;

            // Otherwise:
            // this hitbox can damage anything with IDamageable.
            return true;
        }

        private bool IsPlayer(Node body)
        {
            if (body == null)
                return false;

            if (body.IsInGroup(PlayerGroupName))
                return true;

            Node owner = body.Owner;

            if (owner != null && owner.IsInGroup(PlayerGroupName))
                return true;

            Node parent = body.GetParent();

            while (parent != null)
            {
                if (parent.IsInGroup(PlayerGroupName))
                    return true;

                parent = parent.GetParent();
            }

            return false;
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

        private IKnockable FindKnockable(Node root)
        {
            if (root == null)
                return null;

            if (root is IKnockable knockable)
                return knockable;

            foreach (Node child in root.GetChildren())
            {
                if (child is IKnockable childKnockable)
                    return childKnockable;
            }

            return null;
        }

        private void ApplyKnockbackTo(Node3D body)
        {
            IKnockable knockable = FindKnockable(body);

            if (knockable == null)
                return;

            Vector3 sourcePosition = KnockbackSource != null
                ? KnockbackSource.GlobalPosition
                : GlobalPosition;

            Vector3 direction = body.GlobalPosition - sourcePosition;
            direction.Y = 0f;

            if (direction.LengthSquared() <= 0.001f)
            {
                direction = -GlobalBasis.Z;
                direction.Y = 0f;
            }

            direction = direction.Normalized();
            direction.Y = UpwardKnockback;

            knockable.ApplyKnockback(direction * KnockbackForce);
        }
    }
}