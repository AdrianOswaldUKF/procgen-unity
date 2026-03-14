using UnityEngine;

namespace PerlinNoise
{
    public abstract class PnGenerator : MonoBehaviour
    {
        [Header("Grid")] public int width = 64, height = 64;

        [Header("Perlin Noise")] public int scale = 8;
        public float offsetX, offsetY;

        [Header("Output")] public Transform parent;

        protected abstract string TelemetryName { get; }
        protected abstract void GenerateGrid();

        [ContextMenu("Generate")]
        public void Generate()
        {
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            ClearParent();
            Telemetry.Instance?.RecordGenerationStart(TelemetryName);
            GenerateGrid();
            Telemetry.Instance?.RecordGenerationEnd(TelemetryName);
        }

        [ContextMenu("Clear Parent")]
        public void ClearParentContext()
        {
            ClearParent();
        }

        protected void ClearParent()
        {
            if (parent == null) return;
            while (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                if (Application.isPlaying) 
                    Destroy(child.gameObject);
                else 
                    DestroyImmediate(child.gameObject);
            }
        }

        protected virtual void Start()
        {
            Generate();
        }
    }
}