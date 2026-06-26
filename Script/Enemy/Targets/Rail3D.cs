using Godot;

namespace Game.Targets
{
    public partial class Rail3D : Node3D
    {
        [ExportGroup("Debug")]
        [Export] public bool DrawDebug = true;
        [Export] public Color DebugColor = Colors.Yellow;
        [Export] public int DebugSegments = 64;
        [Export] public float DebugLineWidth = 0.05f;

        private MeshInstance3D _debugMeshInstance;
        private ImmediateMesh _debugMesh;
        private StandardMaterial3D _debugMaterial;

        public override void _Ready()
        {
            if (DrawDebug)
                CreateDebugMesh();
        }

        public virtual Vector3 GetLocalPoint(float progress)
        {
            return Vector3.Zero;
        }

        public Vector3 GetGlobalPoint(float progress)
        {
            return GlobalTransform * GetLocalPoint(WrapProgress(progress));
        }

        protected float WrapProgress(float progress)
        {
            progress %= 1.0f;

            if (progress < 0.0f)
                progress += 1.0f;

            return progress;
        }

        protected virtual bool IsClosedLoop()
        {
            return true;
        }

        private void CreateDebugMesh()
        {
            _debugMeshInstance = new MeshInstance3D();
            _debugMeshInstance.Name = "DebugRailMesh";
            AddChild(_debugMeshInstance);

            _debugMesh = new ImmediateMesh();
            _debugMeshInstance.Mesh = _debugMesh;

            _debugMaterial = new StandardMaterial3D
            {
                AlbedoColor = DebugColor,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                NoDepthTest = true
            };

            _debugMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, _debugMaterial);

            int segments = Mathf.Max(DebugSegments, 2);

            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments;
                float b = (i + 1) / (float)segments;

                if (!IsClosedLoop() && i == segments - 1)
                    break;

                Vector3 pointA = GetLocalPoint(a);
                Vector3 pointB = GetLocalPoint(b);

                _debugMesh.SurfaceAddVertex(pointA);
                _debugMesh.SurfaceAddVertex(pointB);
            }

            _debugMesh.SurfaceEnd();
        }
    }
}