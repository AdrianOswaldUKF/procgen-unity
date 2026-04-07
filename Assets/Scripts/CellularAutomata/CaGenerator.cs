using System.Collections.Generic;
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
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;
        public TMP_InputField cellSizeInput;
        public TMP_InputField seedInput;
        public Slider fillProbabilitySlider;
        public TMP_InputField iterationsInput;
        public TMP_InputField birthThresholdInput;

        [Header("Output")] 
        public Transform parent;

        protected bool[,] grid;
        protected bool[,] buffer;

        protected abstract float DefaultFillProbability {get; }
        protected abstract int DefaultIterations { get; }
        protected abstract int DefaultBirthThreshold { get; }
        protected abstract string TelemetryName { get; }

        [ContextMenu("Generate")]
        public void Generate()
        {
            ReadUIInputs();
            Metrics.Instance?.StartPcg(TelemetryName, width, height);

            InitializeGrid();
            GenerateGrid();
            ApplyIterations();
            
            Metrics.Instance?.LogCa(
                deadEnds: CountDeadEnds(grid),
                regions: CountRegions(grid), 
                fillPct: CalculateFillPct(grid),
                iterations: DefaultIterations,
                birthThreshold: DefaultBirthThreshold
            );
            
            ClearPrefabs();
            PlacePrefabs();
            Metrics.Instance?.EndPcg();
        }
        
        [ContextMenu("Clear Parent")]
        public void ClearParentContext()
        {
            ClearPrefabs();
        }

        protected virtual void ReadUIInputs()
        {
            if (!Application.isPlaying) 
                return;
            
            if (!string.IsNullOrEmpty(widthInput.text))
                width = int.Parse(widthInput.text);
            
            if (!string.IsNullOrEmpty(heightInput.text))
                height = int.Parse(heightInput.text);
            
            if (!string.IsNullOrEmpty(cellSizeInput.text))
                cellSize = float.Parse(cellSizeInput.text);
            
            seed = seedInput?.text ?? "";
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

                    int nx = cx + ox;
                    int ny = cy + oy;
                    
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height && grid[nx, ny])
                        count++;
                }
            }

            return count;
        }

        protected abstract void PlacePrefabs();

        protected Vector3 GridToWorld(int x, int y)
        {
            Vector3 origin = parent != null ? parent.position : Vector3.zero;
            
            return origin + new Vector3(
                -width * cellSize * 0.5f + x * cellSize,
                0f,
                -height * cellSize * 0.5f + y * cellSize
            );
        }

        private void ClearPrefabs()
        {
            if (parent == null)
                return;
            
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        
        protected int CountDeadEnds(bool[,] grid)
        {
            int deadEnds = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y]) continue;
                    int neighbors = CountNeighbors(x, y);
                    if (neighbors <= 1) deadEnds++;
                }
            }
            return deadEnds;
        }

        protected int CountRegions(bool[,] grid)
        {
            bool[,] visited = new bool[width, height];
            int regions = 0;
    
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] && !visited[x, y])
                    {
                        FloodFill(grid, visited, x, y);
                        regions++;
                    }
                }
            }
            return regions;
        }

        private void FloodFill(bool[,] grid, bool[,] visited, int sx, int sy)
        {
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            queue.Enqueue((sx, sy));
            visited[sx, sy] = true;
    
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                int[] dx = {0, 0, 1, -1};
                int[] dy = {1, -1, 0, 0};
        
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d], ny = y + dy[d];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && 
                        grid[nx, ny] && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }

        protected float CalculateFillPct(bool[,] grid)
        {
            int filled = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y]) filled++;
                }
            }
            return (float)filled / (width * height) * 100f;
        }

        protected virtual void Start()
        {
            SetupUI();
        }

        protected virtual void SetupUI() {}
    }
}