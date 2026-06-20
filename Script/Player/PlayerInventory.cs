using Godot;
using System;
using Game.Items;

public partial class PlayerInventory : Node, IItemReceiver
{
    public event Action<int> StarCountChanged;

    public int StarCount { get; private set; }

    public bool CanReceiveItem(ItemData item, int amount)
    {
        return true;
    }

    public void ReceiveItem(ItemData item, int amount)
    {
        GD.Print($"Received {amount}x {item.DisplayName}");

        if (item.Id == "star")
        {
            StarCount += amount;

            StarCountChanged?.Invoke(StarCount);
        }
    }
}