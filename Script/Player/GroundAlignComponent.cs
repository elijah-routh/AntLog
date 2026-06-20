using Godot;

public partial class GroundAlignComponent : Node
{
    [Export] public Node3D VisualRoot;
    [Export] public float AlignSpeed = 12f;

    public void AlignToGround(CharacterBody3D body, float delta)
    {
        if (VisualRoot == null)
            return;

        Vector3 targetUp = body.IsOnFloor()
            ? body.GetFloorNormal().Normalized()
            : Vector3.Up;

        Vector3 forward = -body.GlobalBasis.Z;
        forward = forward.Slide(targetUp);

        if (forward.LengthSquared() < 0.001f)
        {
            forward = -VisualRoot.GlobalBasis.Z;
            forward = forward.Slide(targetUp);
        }

        if (forward.LengthSquared() < 0.001f)
            return;

        forward = forward.Normalized();

        Vector3 right = forward.Cross(targetUp).Normalized();

        Basis targetBasis = new Basis(
            right,
            targetUp,
            -forward
        ).Orthonormalized();

        Quaternion currentRotation = VisualRoot.GlobalBasis.Orthonormalized().GetRotationQuaternion().Normalized();
        Quaternion targetRotation = targetBasis.GetRotationQuaternion().Normalized();

        float weight = Mathf.Clamp(AlignSpeed * delta, 0f, 1f);

        Quaternion finalRotation = currentRotation.Slerp(targetRotation, weight).Normalized();

        VisualRoot.GlobalBasis = new Basis(finalRotation);
    }
}