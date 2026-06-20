using Godot;

namespace Game.Items;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string Id = "";
    [Export] public string DisplayName = "";

    [ExportGroup("Stacking")]
    [Export] public int MaxStackSize = 99;

    [ExportGroup("World Visuals")]
    [Export] public bool SpinsInWorld = true;

    [Export(PropertyHint.Range, "0,1000")]
    public float SpinSpeed = 180f;
}