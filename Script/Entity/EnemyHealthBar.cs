using Godot;
using Game.Entity;

namespace Game.Enemy
{
    public partial class EnemyHealthBar : Node3D
    {
        [Export] public HealthBar HealthBar { get; set; }

        public override void _Ready()
        {
            if (HealthBar == null)
                HealthBar = GetNodeOrNull<HealthBar>("SubViewport/HealthBar");
        }

        public void InitHealth(float maxHealth)
        {
            if (HealthBar == null)
                return;

            HealthBar.InitHealth(maxHealth);
            HealthBar.MaxValue = maxHealth;
            HealthBar.Health = maxHealth;
        }

        public void SetHealth(float currentHealth, float maxHealth)
        {
            if (HealthBar == null)
                return;

            HealthBar.MaxValue = maxHealth;
            HealthBar.Health = currentHealth;
        }

        public void HideBar()
        {
            Visible = false;
        }

        public void ShowBar()
        {
            Visible = true;
        }
    }
}