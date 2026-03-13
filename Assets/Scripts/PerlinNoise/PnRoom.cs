using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoise
{
    public class PnRoom : MonoBehaviour
    {
        [Header("Grid Size")] 
        public int width = 32;
        public int height = 32;

        [Header("Perlin Noise")] 
        public int scale = 8;
        public float roomThreshold = 0.2f;
        public float wallThreshold = 0.8f;
        public float offsetX;
        public float offsetY;

        [Header("Room Size")] 
        public float cellSize = 4f;

        [Header("Prefabs")] 
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public Transform parent;

        [ContextMenu("Generate")]
        public void Generate()
        {
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            ClearParent();
            GenerateRooms();
        }

        [ContextMenu("ClearParent")]
        public void ClearContext()
        {
            ClearParent();
        }

        void Start()
        {
            Telemetry.Instance?.RecordGenerationStart("PerlinNoiseRoom");
            Generate();
            Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseRoom");
        }

        void GenerateRooms()
        {
            float originX = -width * cellSize / 2f;
            float originZ = -height * cellSize / 2f;

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

                    GameObject prefab = (!isWall && isRoom) ? floorPrefab : wallPrefab;
                    Vector3 pos = new Vector3(originX + x * cellSize, 0, originZ + y * cellSize);

                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent);

                    if (prefab == floorPrefab)
                    {
                        obj.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                        obj.transform.position = new Vector3(pos.x, 0, pos.z);
                    }
                    else
                    {
                        float wallHeight = 2.5f;
                        obj.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
                        obj.transform.position = new Vector3(pos.x, wallHeight * 0.5f, pos.z);
                    }
                }
            }
        }


        void ClearParent()
        {
            if (parent == null) return;
            while (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}