using Godot;

namespace Game.Enemy
{
    public class BigBlobDeadState : EnemyStateBase
    {
        private readonly BigBlobEnemyController _blobController;
        private float _despawnTimer = 2.0f;

        public BigBlobDeadState(
            BigBlobEnemyController controller,
            EnemyBase enemy)
            : base(controller, enemy)
        {
            _blobController = controller;
        }

        public override void Enter()
        {
            Enemy.Movement.Stop();

            _blobController.AnimationController?.PlayDeath();

            GD.Print($"{Enemy.Name}: Enter BigBlob Dead");
        }

        public override void Exit()
        {
        }

        public override void Update(double delta)
        {
            _despawnTimer -= (float)delta;

            if (_despawnTimer <= 0f)
                Enemy.QueueFree();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.Movement.Stop();
        }
    }
}