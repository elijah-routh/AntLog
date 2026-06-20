namespace Game.Entity
{
    public interface IState
    {
        void Enter();
        void Exit();
        void Update(double delta);
        void PhysicsUpdate(double delta);
    }
}