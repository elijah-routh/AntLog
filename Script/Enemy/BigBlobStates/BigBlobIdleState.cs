using Godot;

namespace Game.Enemy
{
    public class BigBlobIdleState : EnemyStateBase
    {
        private readonly BigBlobEnemyController _blobController;

        public BigBlobIdleState(
            BigBlobEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _blobController = controller;
        }

        public override void Enter()
        {
            Enemy.Movement.Stop();

            _blobController.AnimationController?.PlayIdle();

            GD.Print($"{Enemy.Name}: Enter BigBlob Idle");
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