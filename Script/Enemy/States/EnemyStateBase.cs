using Game.Entity;

namespace Game.Enemy
{
    public abstract class EnemyStateBase : StateBase
    {
        protected EnemyControllerBase Controller { get; }
        protected EnemyBase Enemy { get; }

        protected EnemyStateBase(
            EnemyControllerBase controller,
            EnemyBase enemy)
        {
            Controller = controller;
            Enemy = enemy;
        }
    }
}