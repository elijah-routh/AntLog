using Godot;

namespace Game.Enemy
{
    public class DinoChargeState : EnemyStateBase
    {
        private readonly DinoEnemyController _dinoController;
        private readonly Node3D _target;

        private Vector3 _chargeDirection;
        private float _chargeDuration;
        private float _timer;

        public DinoChargeState(
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
            GD.Print($"{Enemy.Name}: Enter Dino Charge");

            _timer = 0f;
            _chargeDuration = _dinoController.GetRandomChargeDuration();

            _chargeDirection = _dinoController.GetDirectionToTarget();

            if (_chargeDirection == Vector3.Zero)
                _chargeDirection = _dinoController.GetForwardDirection();

            Enemy.Movement.SetSpeedMultiplier(_dinoController.ChargeSpeedMultiplier);

            _dinoController.RegisterChargeUsed();
            _dinoController.StartChargeCooldown();

            _dinoController.Animations?.PlayRun();
        }

        public override void PhysicsUpdate(double delta)
        {
            _timer += (float)delta;

            if (IsEdgeAhead())
            {
                StopCharge();

                _dinoController.ChangeState(
                    new DinoTiredState(_dinoController, Enemy, _target)
                );

                return;
            }

            // Do not face the target here.
            // This is intentional: the dino commits to the charge direction.
            Enemy.Movement.Move(_chargeDirection);

            if (_timer >= _chargeDuration)
            {
                StopCharge();

                if (_dinoController.HasUsedAllCharges())
                {
                    _dinoController.ChangeState(
                        new DinoTiredState(_dinoController, Enemy, _target)
                    );
                }
                else
                {
                    _dinoController.ChangeState(
                        new DinoRunState(_dinoController, Enemy, _target)
                    );
                }
            }
        }

        public override void Exit()
        {
            StopCharge();
        }

        private bool IsEdgeAhead()
        {
            if (Enemy.EdgeCheck == null)
                return false;

            return !Enemy.EdgeCheck.CanMove(_chargeDirection);
        }

        private void StopCharge()
        {
            Enemy.Movement.ResetSpeedMultiplier();
            Enemy.Movement.Stop();
        }
    }
}