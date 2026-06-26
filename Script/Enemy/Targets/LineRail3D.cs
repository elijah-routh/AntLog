using Godot;

namespace Game.Targets
{
    public partial class LineRail3D : Rail3D
    {
        [ExportGroup("Line")]
        [Export] public Vector3 StartPoint = new Vector3(-5.0f, 0.0f, 0.0f);
        [Export] public Vector3 EndPoint = new Vector3(5.0f, 0.0f, 0.0f);

        public override Vector3 GetLocalPoint(float progress)
        {
            return StartPoint.Lerp(EndPoint, progress);
        }

        protected override bool IsClosedLoop()
        {
            return false;
        }
    }
}