using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CA_Generation : MonoBehaviour
{
    [Header("Grid")]
    public int width = 50;
    public int height = 50;
    public float cellSize = 1f;
    public string seed;

    [Header("Cellular Automata")]
    [Range(0f, 1f)]
    public float fillProbability = 0.65f;
    public int iterations = 5;
    public int birthThreshold = 5;
    
    public TMP_InputField seedInput;
    public Slider fillProbabilitySlider;
    public TMP_InputField iterationsInput;
    public TMP_InputField birthThresholdInput;

    [Header("Prefab")]
    public GameObject prefab;
    public Transform parent;

    private bool[,] _grid;
    private bool[,] _buffer;
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        if (seedInput.text != "")
        {
            seed = seedInput.text;
        }

        if (fillProbabilitySlider.value >= 0f && fillProbabilitySlider.value <= 1f)
        {
            fillProbability = fillProbabilitySlider.value;
        }
        if (iterationsInput.text != "")
        {
            iterations = int.Parse(iterationsInput.text);
        }

        if (birthThresholdInput.text != "")
        {
            birthThreshold = int.Parse(birthThresholdInput.text);
        }
        Telemetry.Instance?.RecordGenerationStart("CellularAutomataGeneration");
        Initialize();
        GenerateGrid();
        ApplyIterations();
        ClearPrefabs();
        PlacePrefabs();
        Telemetry.Instance?.RecordGenerationEnd("CellularAutomataGeneration");
    }

    void Start()
    {
        seedInput.text = seed;
        fillProbabilitySlider.value = fillProbability;
        iterationsInput.text = iterations.ToString();
        birthThresholdInput.text = birthThreshold.ToString();
    }

    private void Initialize()
    {
        _grid = new bool[width, height];
        _buffer = new bool[width, height];
    }

    private void GenerateGrid()
    {
        if (!string.IsNullOrEmpty(seed))
            Random.InitState(seed.GetHashCode());

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

                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (_grid[nx, ny]) count++;
            }
        }

        return count;
    }

    private void PlacePrefabs()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!_grid[x, y])
                {
                    continue;
                }

                Vector3 pos = GridToWorld(x, y);
                GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);
                go.transform.localScale = Vector3.one * cellSize;
            }
        }
    }
    
    private void ClearPrefabs()
    {
        if (parent != null)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        else
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private Vector3 GridToWorld(int x, int y)
    {
        float originX = -width * cellSize * 0.5f;
        float originZ = -height * cellSize * 0.5f;

        return new Vector3(
            originX + x * cellSize,
            0f,
            originZ + y * cellSize
        );
    }
}
