using UnityEngine;
using Random = UnityEngine.Random;

public class CA_Texture : MonoBehaviour
{
    [Header("Grid")]
    public int width = 100;
    public int height = 100;
    public string seed;

    [Header("Cellular Automata")]
    [Range(0f, 1f)]
    public float fillProbability = 0.65f;
    public int iterations = 5;
    public int birthThreshold = 5;

    [Header("Display")]
    public Renderer targetRenderer;
    public Color aliveColor = Color.black;
    public Color deadColor = Color.white;

    private bool[,] _grid;
    private bool[,] _buffer;
    private Texture2D _texture;
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        Telemetry.Instance?.RecordGenerationStart("CellularAutomataTexture");
        Initialize();
        GenerateGrid();
        ApplyIterations();
        UpdateTexture();
        Telemetry.Instance?.RecordGenerationEnd("CellularAutomataTexture");
    }

    void Start()
    {
        Generate();
    }

    private void Initialize()
    {
        _grid = new bool[width, height];
        _buffer = new bool[width, height];

        _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;

        if (targetRenderer != null)
        {
            Material mat = new Material(targetRenderer.sharedMaterial);
            mat.mainTexture = _texture;
            targetRenderer.sharedMaterial = mat;
        }
    }

    private void GenerateGrid()
    {
        if (!string.IsNullOrEmpty(seed))
        {
            Random.InitState(seed.GetHashCode());
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = Random.value < fillProbability;
            }
        }
    }

    private void ApplyIterations()
    {
        for (int i = 0; i < iterations; i++)
        {
            Step();
        }
    }

    private void Step()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbors = CountNeighbors(x, y);
                _buffer[x, y] = neighbors >= birthThreshold;
            }
        }

        (_grid, _buffer) = (_buffer, _grid);
    }

    private int CountNeighbors(int cx, int cy)
    {
        int count = 0;

        for (int ox = -1; ox <= 1; ox++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                if (ox == 0 && oy == 0)
                {
                    continue;
                }

                int nx = cx + ox;
                int ny = cy + oy;

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }
                if (_grid[nx, ny])
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void UpdateTexture()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _texture.SetPixel(x, y, _grid[x, y] ? aliveColor : deadColor);
            }
        }

        _texture.Apply();
    }
}
