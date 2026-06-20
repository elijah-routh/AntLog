using Godot;


namespace Game.Enemy
{
    public class IdleState : EnemyStateBase
    {
        public IdleState(
            EnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
        }

        public override void Enter()
        {
            Enemy.Movement.Stop();

            GD.Print($"{Enemy.Name}: Enter Idle");
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