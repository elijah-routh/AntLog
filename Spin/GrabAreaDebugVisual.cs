using Godot;

public partial class GrabAreaDebugVisual : MeshInstance3D
{
    [Export] public PlayerMoveComponent Movement;

    [ExportGroup("Colors")]
    [Export] public Color InactiveColor = new Color(1f, 1f, 1f, 0.15f);
    [Export] public Color ActiveColor = new Color(1f, 0f, 0f, 0.65f);

    private StandardMaterial3D _material;

    public override void _Ready()
    {
        _material = new StandardMaterial3D();
        _material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        _material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        _material.AlbedoColor = InactiveColor;

        MaterialOverride = _material;
        Visible = true;
    }

    public override void _Process(double delta)
    {
        if (_material == null || Movement == null)
            return;

        bool active = Movement.IsDiving;

        _material.AlbedoColor = active ? ActiveColor : InactiveColor;
        _material.EmissionEnabled = active;
        _material.Emission = active ? new Color(1f, 0f, 0f) : Colors.Black;
        _material.EmissionEnergyMultiplier = active ? 2.0f : 0.0f;
    }
}