using UnityEngine;

namespace CellularAutomata
{
    public class CaBiome : CaGenerator
    {
        [Header("Biome")]
        public Terrain terrain;
        private TerrainData _terrainData;
        private int _heightRes;
        private int _alphaRes;

        [Header("CA Settings")]
        public float fillProbability = 0.5f;
        public int iterations = 3;
        public int birthThreshold = 4;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaBiome";

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
                Debug.LogWarning("[CaBiome] Terrain not assigned.");
                return;
            }

            SetupTerrainData();
            float[,,] alphaMaps = new float[_alphaRes, _alphaRes, 2];

            for (int ax = 0; ax < _alphaRes; ax++)
            {
                for (int ay = 0; ay < _alphaRes; ay++)
                {
                    int gx = Mathf.FloorToInt((float)ax / _alphaRes * width);
                    int gy = Mathf.FloorToInt((float)ay / _alphaRes * height);

                    gx = Mathf.Clamp(gx, 0, width - 1);
                    gy = Mathf.Clamp(gy, 0, height - 1);

                    if (grid[gx, gy])
                    {
                        alphaMaps[ay, ax, 0] = 1f;
                        alphaMaps[ay, ax, 1] = 0f;
                    }
                    else
                    {
                        alphaMaps[ay, ax, 0] = 0f;
                        alphaMaps[ay, ax, 1] = 1f;
                    }
                }
            }

            _terrainData.SetAlphamaps(0, 0, alphaMaps);
        }
        
        [ContextMenu("Reset Terrain")]
        private void ResetTerrain()
        {
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
            _alphaRes = _terrainData.alphamapResolution;
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