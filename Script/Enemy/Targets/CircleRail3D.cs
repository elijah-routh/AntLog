using Godot;

namespace Game.Targets
{
    public partial class CircleRail3D : Rail3D
    {
        [ExportGroup("Circle")]
        [Export] public float Radius = 8.0f;
        [Export] public bool UseXZPlane = true;

        public override Vector3 GetLocalPoint(float progress)
        {
            float angle = progress * Mathf.Tau;

            float x = Mathf.Cos(angle) * Radius;
            float z = Mathf.Sin(angle) * Radius;

            if (UseXZPlane)
                return new Vector3(x, 0.0f, z);

            return new Vector3(x, z, 0.0f);
        }
    }
}