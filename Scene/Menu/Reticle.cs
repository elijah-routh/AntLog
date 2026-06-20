using Godot;

public partial class Reticle : Control
{
    [Export] public Camera3D Camera;
    [Export] public float AimDistance = 5000f;

    [Export] public float Gap = 8f;
    [Export] public float LineLength = 14f;
    [Export] public float Thickness = 2f;
    [Export] public float DotRadius = 2f;
    [Export] public Color ReticleColor = Colors.White;

    public Vector3 AimPoint { get; private set; }
    public Vector3 AimDirection { get; private set; }

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        UpdateAimPoint();
        QueueRedraw();
    }

    private void UpdateAimPoint()
    {
        Vector2 screenCenter = GetViewportRect().Size / 2f;

        Vector3 rayOrigin = Camera.ProjectRayOrigin(screenCenter);
        Vector3 rayDirection = Camera.ProjectRayNormal(screenCenter);

        PhysicsDirectSpaceState3D space = Camera.GetWorld3D().DirectSpaceState;

        var query = PhysicsRayQueryParameters3D.Create(
            rayOrigin,
            rayOrigin + rayDirection * AimDistance
        );

        var hit = space.IntersectRay(query);

        AimPoint = hit.Count > 0
            ? hit["position"].AsVector3()
            : rayOrigin + rayDirection * AimDistance;

        AimDirection = rayDirection;
    }

    public override void _Draw()
    {
        Vector2 c = Size / 2f;

        DrawLine(c + new Vector2(-Gap - LineLength, 0), c + new Vector2(-Gap, 0), ReticleColor, Thickness);
        DrawLine(c + new Vector2(Gap, 0), c + new Vector2(Gap + LineLength, 0), ReticleColor, Thickness);
        DrawLine(c + new Vector2(0, -Gap - LineLength), c + new Vector2(0, -Gap), ReticleColor, Thickness);
        DrawLine(c + new Vector2(0, Gap), c + new Vector2(0, Gap + LineLength), ReticleColor, Thickness);

        DrawCircle(c, DotRadius, ReticleColor);
    }
}