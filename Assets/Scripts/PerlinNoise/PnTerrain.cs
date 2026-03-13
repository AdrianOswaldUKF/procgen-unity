using System;
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
        private int _resolution;
        private TerrainData _terrainData;
        public String seed;

        [Header("Perlin Noise Settings")] 
        public int scale = 5;
        public float heightMultiplier = .1f;
        private float _offsetX;
        private float _offsetY;

        public TMP_InputField seedInput;
        public TMP_InputField scaleInput;
        public TMP_InputField heightInput;

        [ContextMenu("Generate")]
        public void Generate()
        {
            _terrainData = terrain.terrainData;
            _resolution = _terrainData.heightmapResolution;
            terrainObject.SetActive(true);
            if (seedInput.text != "")
            {
                seed = seedInput.text;
            }

            if (scaleInput.text != "")
            {
                scale = int.Parse(scaleInput.text);
            }

            if (heightInput.text != "")
            {
                heightMultiplier = float.Parse(heightInput.text);
            }

            Telemetry.Instance?.RecordGenerationStart("PerlinNoiseTerrain");
            if (terrain == null)
            {
                terrain = GetComponent<Terrain>();
            }

            GenerateNoise();
            Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseTerrain");
        }

        private void Start()
        {
            seedInput.text = seed;
            scaleInput.text = scale.ToString();
            heightInput.text = heightMultiplier.ToString();

            _terrainData = terrain.terrainData;
            _resolution = _terrainData.heightmapResolution;
            float[,] heights = new float[_resolution, _resolution];
            _terrainData.SetHeights(0, 0, heights);
        }

        private void GenerateNoise()
        {
            if (seed.Length != 0)
            {
                Random.InitState(seed.GetHashCode());
            }

            _offsetX = Random.Range(0f, 99999f);
            _offsetY = Random.Range(0f, 99999f);

            float[,] heights = new float[_resolution, _resolution];

            for (int x = 0; x < _resolution; x++)
            {
                for (int y = 0; y < _resolution; y++)
                {
                    float xCoord = (float)x / _resolution * scale + _offsetX;
                    float yCoord = (float)y / _resolution * scale + _offsetY;

                    float noise = Mathf.PerlinNoise(xCoord, yCoord);
                    heights[x, y] = noise * heightMultiplier;
                }
            }

            _terrainData.SetHeights(0, 0, heights);
        }
    }
}