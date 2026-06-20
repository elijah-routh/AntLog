namespace Game.Entity
{
    public abstract class StateBase : IState
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
    }
}