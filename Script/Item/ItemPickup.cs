using Godot;

namespace Game.Items;

public partial class ItemPickup : Area3D
{
    [Export] public ItemData Item;
    [Export] public int Amount = 1;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (Item == null)
            return;

        if (Item.SpinsInWorld)
        {
            RotateY(Mathf.DegToRad(Item.SpinSpeed) * (float)delta);
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print($"Touched by: {body.Name}");

        PlayerInventory inventory = body.GetNodeOrNull<PlayerInventory>("Inventory");

        if (inventory == null)
        {
            GD.Print("No inventory found.");
            return;
        }

        inventory.ReceiveItem(Item, Amount);
        QueueFree();
    }
}