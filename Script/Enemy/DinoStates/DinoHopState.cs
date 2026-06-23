using Godot;

namespace Game.Enemy
{
    public class DinoHopState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private Vector3 _hopDirection;
        private float _timer;

        public DinoHopState(
            DinoEnemyController controller,
            EnemyBase enemy,
            Node3D target)
            : base(controller, enemy)
        {
            _dinoController = controller;
            _target = target;
        }

        public override void Enter()
        {
            GD.Print($"{Enemy.Name}: Enter Dino Hop");

            _timer = 0f;

            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();

            _hopDirection = PickHopDirection();

            _dinoController.Animations?.PlayJump();
        }

        public override void PhysicsUpdate(double delta)
        {
            _timer += (float)delta;

            // During the hop, keep facing the player.
            // The dino is lining up its charge.
            _dinoController.FaceTarget(delta, _dinoController.RunRotationSpeed);

            if (_hopDirection == Vector3.Zero)
            {
                Enemy.Movement.Stop();
            }
            else
            {
                Enemy.Movement.SetSpeedMultiplier(
                    _dinoController.ChargeHopSideSpeedMultiplier
                );

                Enemy.Movement.Move(_hopDirection);
            }

            if (_timer >= _dinoController.ChargeHopDuration)
            {
                Enemy.Movement.ResetSpeedMultiplier();
                Enemy.Movement.Stop();

                _dinoController.ChangeState(
                    new DinoChargeState(_dinoController, Enemy, _target)
                );
            }
        }

        public override void Exit()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }

        private Vector3 PickHopDirection()
        {
            float roll = GD.Randf();

            if (roll <= _dinoController.HopInPlaceChance)
                return Vector3.Zero;

            Vector3 toTarget = _dinoController.GetDirectionToTarget();

            if (toTarget == Vector3.Zero)
                return Vector3.Zero;

            // Perpendicular directions on the XZ plane.
            Vector3 right = new Vector3(toTarget.Z, 0f, -toTarget.X).Normalized();
            Vector3 left = -right;

            return GD.Randf() > 0.5f ? right : left;
        }
    }
}