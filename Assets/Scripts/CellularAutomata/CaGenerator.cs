using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace CellularAutomata
{
    public abstract class CaGenerator : MonoBehaviour
    {
        [Header("Grid")] 
        public int width = 50;
        public int height = 50;
        public float cellSize = 1f;
        public string seed;

        [Header("UI")] 
        public TMP_InputField seedInput;
        public Slider fillProbabilitySlider;
        public TMP_InputField iterationsInput;
        public TMP_InputField birthThresholdInput;

        [Header("Output")] 
        public Transform parent;

        protected bool[,] grid;
        protected bool[,] buffer;

        protected abstract float DefaultFillProbability { get; }
        protected abstract int DefaultIterations { get; }
        protected abstract int DefaultBirthThreshold { get; }
        protected abstract string TelemetryName { get; }

        [ContextMenu("Generate")]
        public void Generate()
        {
            ReadUIInputs();
            Telemetry.Instance?.RecordGenerationStart(TelemetryName);

            InitializeGrid();
            GenerateGrid();
            ApplyIterations();
            ClearPrefabs();
            PlacePrefabs();

            Telemetry.Instance?.RecordGenerationEnd(TelemetryName);
        }

        protected virtual void ReadUIInputs()
        {
            if (Application.isPlaying)
            {
                if (seedInput != null && seedInput.text != "")
                {
                    seed = seedInput.text;
                }
            }
        }

        protected virtual void InitializeGrid()
        {
            grid = new bool[width, height];
            buffer = new bool[width, height];
        }

        protected virtual void GenerateGrid()
        {
            if (!string.IsNullOrEmpty(seed))
                Random.InitState(seed.GetHashCode());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = Random.value < DefaultFillProbability;
                }
            }
        }

        protected virtual void ApplyIterations()
        {
            for (int i = 0; i < DefaultIterations; i++)
            {
                Step();
            }
        }

        protected virtual void Step()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbors = CountNeighbors(x, y);
                    buffer[x, y] = neighbors >= DefaultBirthThreshold;
                }
            }

            (grid, buffer) = (buffer, grid);
        }

        protected int CountNeighbors(int cx, int cy)
        {
            int count = 0;
            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0) 
                        continue;
                    
                    int nx = cx + ox, ny = cy + oy;
                    
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height && grid[nx, ny])
                        count++;
                }
            }

            return count;
        }

        protected abstract void PlacePrefabs();

        protected Vector3 GridToWorld(int x, int y)
        {
            return new Vector3(
                -width * cellSize * 0.5f + x * cellSize,
                0f,
                -height * cellSize * 0.5f + y * cellSize
            );
        }

        private void ClearPrefabs()
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        protected virtual void Start()
        {
            SetupUI();
        }

        protected virtual void SetupUI()
        {
        }
    }
}