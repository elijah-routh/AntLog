using Godot;

namespace Game.Enemy
{
    public class DinoDeadState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;

        public DinoDeadState(
            DinoEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _dinoController = controller;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Dead");

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            _dinoController.Animations?.PlayDeath();

            Enemy.SetPhysicsProcess(false);
        }
    }
}