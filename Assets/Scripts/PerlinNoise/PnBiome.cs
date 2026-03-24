using UnityEngine;

namespace PerlinNoise
{
    public class PnBiome : PnGenerator
    {
        [Header("Biome")]
        public Terrain terrain;

        [Header("Perlin Settings")]
        public float heightThreshold = 0.6f;
        public float moistureThreshold = 0.7f;

        private TerrainData _terrainData;
        private int _alphaRes;

        protected override string TelemetryName => "PerlinNoiseBiome";

        protected override void GenerateNoise()
        {
            if (terrain == null)
            {
                Debug.LogError("[PnBiome] Terrain not assigned.");
                return;
            }

            SetupTerrainData();

            float[,,] alphaMaps = new float[_alphaRes, _alphaRes, 3];

            for (int ax = 0; ax < _alphaRes; ax++)
            {
                for (int ay = 0; ay < _alphaRes; ay++)
                {
                    int gx = Mathf.FloorToInt((float)ax / _alphaRes * width);
                    int gy = Mathf.FloorToInt((float)ay / _alphaRes * height);

                    gx = Mathf.Clamp(gx, 0, width - 1);
                    gy = Mathf.Clamp(gy, 0, height - 1);

                    float cordX = (float)gx / width * scale + offsetX;
                    float cordY = (float)gy / height * scale + offsetY;

                    float heightNoise = Mathf.PerlinNoise(cordX, cordY);
                    float moistureNoise = Mathf.PerlinNoise(cordX + 500f, cordY + 500f);
                    
                    alphaMaps[ay, ax, 0] = 0f;
                    alphaMaps[ay, ax, 1] = 0f;
                    alphaMaps[ay, ax, 2] = 0f;
                    
                    if (heightNoise > heightThreshold)
                    {
                        alphaMaps[ay, ax, 1] = 1f;
                    }
                    else if (moistureNoise > moistureThreshold)
                    {
                        alphaMaps[ay, ax, 2] = 1f;
                    }
                    else
                    {
                        alphaMaps[ay, ax, 0] = 1f;
                    }
                }
            }

            _terrainData.SetAlphamaps(0, 0, alphaMaps);
        }
        
        private void SetupTerrainData()
        {
            if (terrain == null)
            {
                Debug.LogWarning("[CaTerrain] Terrain not assigned.");
                return;
            }
            _terrainData = terrain.terrainData;
            _alphaRes = _terrainData.alphamapResolution;
        }
    }
}