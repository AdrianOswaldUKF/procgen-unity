using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoise
{
    public class PnTerrain : MonoBehaviour
    {
        [Header("Terrain")] 
        public GameObject terrainObject;
        public Terrain terrain;
        public string seed = "";

        [Header("Perlin Noise")] 
        public int scale = 5;
        public float heightMultiplier = 0.1f;

        [Header("UI")]
        public TMP_InputField seedInput;
        public TMP_InputField scaleInput;
        public TMP_InputField heightInput;

        private TerrainData _terrainData;
        private int _resolution;

        [ContextMenu("Generate")]
        public void Generate()
        {
            ReadUIInputs();
            Telemetry.Instance?.RecordGenerationStart("PerlinNoiseTerrain");

            if (terrain == null) 
                terrain = GetComponent<Terrain>();
            
            _terrainData = terrain.terrainData;
            _resolution = _terrainData.heightmapResolution;

            terrainObject?.SetActive(true);
            GenerateNoise();

            Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseTerrain");
        }

        private void ReadUIInputs()
        {
            if (!Application.isPlaying) 
                return;
            
            if (seedInput != null && seedInput.text != "") 
                seed = seedInput.text;
            
            if (scaleInput != null && scaleInput.text != "") 
                scale = int.Parse(scaleInput.text);
            
            if (heightInput != null && heightInput.text != "") 
                heightMultiplier = float.Parse(heightInput.text);
        }

        private void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            seedInput.text = seed;
            scaleInput.text = scale.ToString();
            heightInput.text = heightMultiplier.ToString();
        }

        private void GenerateNoise()
        {
            if (!string.IsNullOrEmpty(seed))
                Random.InitState(seed.GetHashCode());

            float offsetX = Random.Range(0f, 99999f);
            float offsetY = Random.Range(0f, 99999f);

            float[,] heights = new float[_resolution, _resolution];
            for (int x = 0; x < _resolution; x++)
            {
                for (int y = 0; y < _resolution; y++)
                {
                    float xCoord = (float)x / _resolution * scale + offsetX;
                    float yCoord = (float)y / _resolution * scale + offsetY;
                    heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * heightMultiplier;
                }
            }
            _terrainData.SetHeights(0, 0, heights);
        }
    }
}