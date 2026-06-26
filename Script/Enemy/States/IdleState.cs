using Godot;


namespace Game.Enemy
{
    public class IdleState : EnemyStateBase
    {
        private readonly BossEnemyController _bossController;

        public IdleState(
            EnemyControllerBase controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _bossController = controller as BossEnemyController;
        }

        public override void Enter()
        {
            Enemy.Movement.Stop();

            GD.Print($"{Enemy.Name}: Enter Idle");

            if (_bossController == null)
            {
                GD.PrintErr("IdleState: controller is not BossEnemyController.");
                return;
            }

            if (_bossController.AnimationController == null)
            {
                GD.PrintErr("IdleState: AnimationController is not assigned on BossEnemyController.");
                return;
            }

            GD.Print("IdleState: Playing idle animation.");
            _bossController.AnimationController.PlayIdle();
        }

        public override void Exit()
        {
        }

        public override void Update(double delta)
        {
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.Movement.Stop();
        }
    }
}