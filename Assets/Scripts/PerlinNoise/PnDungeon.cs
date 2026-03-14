using UnityEngine;

namespace PerlinNoise
{
    public class PnDungeon : PnGenerator
    {
        [Header("Dungeon Size")] 
        public float cellSize = 3f;
        public float wallHeight = 4f;

        [Header("Prefabs")] 
        public GameObject floorPrefab;
        public GameObject wallPrefab;

        protected override string TelemetryName => "PerlinNoiseDungeon";

        protected override void GenerateGrid()
        {
            float originX = -width * cellSize / 2f;
            float originZ = -height * cellSize / 2f;
            float threshold = 0.35f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float cordX = (float)x / width * scale + offsetX;
                    float cordY = (float)y / height * scale + offsetY;

                    float noise = Mathf.PerlinNoise(cordX, cordY);
                    bool isFloor = noise < threshold;

                    GameObject prefab = isFloor ? floorPrefab : wallPrefab;
                    Vector3 pos = new Vector3(originX + x * cellSize, 0, originZ + y * cellSize);

                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent);

                    if (isFloor)
                    {
                        obj.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    }
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