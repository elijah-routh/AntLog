namespace Game.Entity
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public bool IsInState<T>() where T : IState
        {
            return CurrentState is T;
        }

        public void ChangeState(IState newState)
        {
            if (CurrentState?.GetType() == newState.GetType())
                return;

            CurrentState?.Exit();

            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update(double delta)
        {
            CurrentState?.Update(delta);
        }

        public void PhysicsUpdate(double delta)
        {
            CurrentState?.PhysicsUpdate(delta);
        }
    }
}