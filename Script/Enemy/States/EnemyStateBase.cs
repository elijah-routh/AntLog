using Game.Entity;

namespace Game.Enemy
{
    public abstract class EnemyStateBase : StateBase
    {
        protected EnemyController Controller { get; }
        protected EnemyBase Enemy { get; }

        protected EnemyStateBase(
            EnemyController controller,
            EnemyBase enemy)
        {
            Controller = controller;
            Enemy = enemy;
        }
    }
}