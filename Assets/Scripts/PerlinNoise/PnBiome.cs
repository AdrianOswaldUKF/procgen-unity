using UnityEngine;

namespace PerlinNoise
{
    public class PnBiome : PnGenerator
    {
        [Header("Biome")]
        public Terrain terrain;

        [Header("Perlin Settings")]
        public float heightThreshold = 0.6f;
        public float waterThreshold = 0.7f;

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
                    float cordX = (float)ax / width * scale + offsetX;
                    float cordY = (float)ay / height * scale + offsetY;

                    float heightNoise = Mathf.PerlinNoise(cordX, cordY);
                    float waterNoise = Mathf.PerlinNoise(cordX + 500f, cordY + 500f);

                    alphaMaps[ay, ax, 0] = 0f;
                    alphaMaps[ay, ax, 1] = 0f;
                    alphaMaps[ay, ax, 2] = 0f;

                    if (heightNoise > heightThreshold)
                        alphaMaps[ay, ax, 1] = 1f;
                    else if (waterNoise > waterThreshold)
                        alphaMaps[ay, ax, 2] = 1f;
                    else
                        alphaMaps[ay, ax, 0] = 1f;
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
            
            _terrainData.size = new Vector3(width, _terrainData.size.y, height);
            terrain.transform.position = new Vector3(-width / 2f, terrain.transform.position.y, -height / 2f);
        }
    }
}