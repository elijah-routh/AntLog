using Game.Enemy;
using Game.Entity;
using Godot;

namespace Game.World
{
    public partial class FallKillZone : Area3D
    {
        [Export] public bool KillPlayer = true;
        [Export] public bool DespawnEnemies = true;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            AreaEntered += OnAreaEntered;
            Monitoring = true;
        }

        private void OnBodyEntered(Node3D body)
        {
            HandleNode(body);
        }

        private void OnAreaEntered(Area3D area)
        {
            HandleNode(area);
        }

        private void HandleNode(Node node)
        {
            if (node == null)
                return;

            PlayerController player = FindParentOfType<PlayerController>(node);

            if (player != null)
            {
                if (KillPlayer)
                {
                    player.Health?.Kill();
                }

                return;
            }

            if (DespawnEnemies)
            {
                EnemyBase enemy = FindParentOfType<EnemyBase>(node);

                if (enemy != null)
                {
                    enemy.QueueFree();
                    return;
                }
            }
        }

        private T FindParentOfType<T>(Node node) where T : Node
        {
            Node current = node;

            while (current != null)
            {
                if (current is T typedNode)
                    return typedNode;

                current = current.GetParent();
            }

            return null;
        }
    }
}