namespace Game.Items
{
    public interface IUsable
    {
        void Use();
    }

    public interface IStackable
    {
        int MaxStackSize { get; }
    }

    public interface IItemReceiver
    {
        bool CanReceiveItem(ItemData item, int amount);
        void ReceiveItem(ItemData item, int amount);
    }

    public abstract class ItemBase
    {
        public string Name { get; set; }
    }

    public abstract class ConsumableItem : ItemBase, IUsable, IStackable
    {
        public abstract int MaxStackSize { get; }

        public abstract void Use();
    }
}