using Godot;

public partial class HUD : CanvasLayer
{
    private Label _starLabel;
    private PlayerInventory _inventory;

    public override void _Ready()
    {
        _starLabel = GetNode<Label>("StarCount");

        _inventory = GetTree()
            .GetFirstNodeInGroup("player_inventory") as PlayerInventory;

        if (_inventory != null)
        {
            _inventory.StarCountChanged += OnStarCountChanged;

            OnStarCountChanged(_inventory.StarCount);
        }
    }

    private void OnStarCountChanged(int count)
    {
        _starLabel.Text = count.ToString();
    }
}