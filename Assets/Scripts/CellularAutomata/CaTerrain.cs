using UnityEngine;

namespace CellularAutomata
{
    public class CaTerrain : CaGenerator
    {
        [Header("Terrain")]
        public Terrain terrain;
        private TerrainData _terrainData;
        private int _heightRes;

        [Header("CA Settings")]
        public float fillProbability = 0.5f;
        public int iterations = 3;
        public int birthThreshold = 4;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaTerrain";

        protected override void ReadUIInputs()
        {
            base.ReadUIInputs();
            if (Application.isPlaying)
            {
                fillProbability = fillProbabilitySlider?.value ?? fillProbability;

                if (iterationsInput != null && iterationsInput.text != "")
                    iterations = int.Parse(iterationsInput.text);

                if (birthThresholdInput != null && birthThresholdInput.text != "")
                    birthThreshold = int.Parse(birthThresholdInput.text);
            }
        }

        protected override void PlacePrefabs()
        {
            if (terrain == null)
            {
                Debug.LogWarning("[CaTerrain] Terrain not assigned.");
                return;
            }

            SetupTerrainData();
            float[,] heights = new float[_heightRes, _heightRes];

            for (int x = 0; x < _heightRes; x++)
            {
                for (int y = 0; y < _heightRes; y++)
                {
                    int gx = Mathf.FloorToInt((float)x / _heightRes * width);
                    int gy = Mathf.FloorToInt((float)y / _heightRes * height);

                    gx = Mathf.Clamp(gx, 0, width - 1);
                    gy = Mathf.Clamp(gy, 0, height - 1);

                    if (grid[gx, gy])
                    {
                        heights[x, y] = 0.1f;
                    }
                    else
                    {
                        heights[x, y] = 0f;
                    }
                }
            }

            _terrainData.SetHeights(0, 0, heights);
        }
        
        [ContextMenu("Reset Terrain")]
        public void ResetTerrain()
        {
            SetupTerrainData();
            if (_terrainData == null)
                return;

            float[,] heights = new float[_heightRes, _heightRes];
            _terrainData.SetHeights(0, 0, heights);
        }

        private void SetupTerrainData()
        {
            if (terrain == null)
            {
                Debug.LogWarning("[CaTerrain] Terrain not assigned.");
                return;
            }
            _terrainData = terrain.terrainData;
            _heightRes = _terrainData.heightmapResolution;
        }

        protected override void SetupUI()
        {
            seedInput.text = seed;
            fillProbabilitySlider.value = fillProbability;
            iterationsInput.text = iterations.ToString();
            birthThresholdInput.text = birthThreshold.ToString();
        }
    }
}
