namespace Game.Items
{
    public interface IItemEffect
    {
        void Use();
    }

    public abstract class HealEffect : IItemEffect
    {
        public abstract void Use();
    }

    public abstract class BuffEffect : IItemEffect
    {
        public abstract void Use();
    }
}