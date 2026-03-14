using UnityEngine;

namespace PerlinNoise
{
    public class PnRoom : PnGenerator
    {
        [Header("Room Size")] 
        public float cellSize = 4f;
        public float wallHeight = 2.5f;

        [Header("Prefabs")] 
        public GameObject floorPrefab;
        public GameObject wallPrefab;

        protected override string TelemetryName => "PerlinNoiseRoom";

        protected override void GenerateGrid()
        {
            float originX = -width * cellSize / 2f;
            float originZ = -height * cellSize / 2f;
            float roomThreshold = 0.2f;
            float wallThreshold = 0.8f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float cordX = (float)x / width * scale + offsetX;
                    float cordY = (float)y / height * scale + offsetY;

                    float roomNoise = Mathf.PerlinNoise(cordX, cordY);
                    float wallNoise = Mathf.PerlinNoise(cordX + 1000, cordY + 1000);

                    bool isRoom = roomNoise < roomThreshold;
                    bool isWall = wallNoise > wallThreshold;

                    GameObject prefab = !isWall && isRoom ? floorPrefab : wallPrefab;
                    Vector3 pos = new Vector3(originX + x * cellSize, 0, originZ + y * cellSize);

                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent);

                    if (prefab == floorPrefab)
                        obj.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    else
                    {
                        obj.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
                        obj.transform.position += Vector3.up * wallHeight * 0.5f;
                    }
                }
            }
        }
    }
}