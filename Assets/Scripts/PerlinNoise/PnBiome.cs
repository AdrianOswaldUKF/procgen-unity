using UnityEngine;

namespace PerlinNoise
{
    public class PnBiome : PnGenerator
    {
        [Header("Prefabs")]
        public GameObject grassPrefab;
        public GameObject rockPrefab;
        public GameObject waterPrefab;

        protected override string TelemetryName => "PerlinNoiseBiome";

        protected override void GenerateGrid()
        {
            float originX = -width / 2f;
            float originZ = -height / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float cordX = (float)x / width * scale + offsetX;
                    float cordY = (float)y / height * scale + offsetY;

                    float heightNoise = Mathf.PerlinNoise(cordX, cordY);
                    float moistureNoise = Mathf.PerlinNoise(cordX + 500, cordY + 500);

                    GameObject prefab = heightNoise > 0.6f ? rockPrefab : moistureNoise > 0.7f ? waterPrefab : grassPrefab;

                    Vector3 pos = new Vector3(originX + x, 0, originZ + y);
                    Instantiate(prefab, pos, Quaternion.identity, parent);
                }
            }
        }
    }
}