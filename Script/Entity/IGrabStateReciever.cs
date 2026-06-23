namespace Game.Enemy
{
    public interface IGrabStateReceiver
    {
        void OnGrabbed();
        void OnReleased();
        void OnThrown();
        void OnThrowFinished();
    }
}

//Needed so statemachine knows when to enter grab state without requiring enemycontrollerbase implementing grab