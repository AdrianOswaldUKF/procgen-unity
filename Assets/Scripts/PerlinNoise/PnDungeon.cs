using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoise
{
    public class PnDungeon : MonoBehaviour
    {
        [Header("Grid Size")] 
        public int width = 128;
        public int height = 128;

        [Header("Perlin Noise")] 
        public int scale = 3;
        public float threshold = 0.35f;
        public float offsetX;
        public float offsetY;

        [Header("Dungeon Size")] 
        public float cellSize = 3f;
        public float wallHeight = 4f;

        [Header("Prefabs")] 
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public Transform parent;

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (parent != null) parent.localScale = Vector3.one;
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            ClearParent();
            GenerateDungeon();
        }

        [ContextMenu("ClearParent")]
        public void ClearContext()
        {
            ClearParent();
        }

        void Start()
        {
            Telemetry.Instance?.RecordGenerationStart("PerlinNoiseDungeon");
            Generate();
            Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseDungeon");
        }

        void GenerateDungeon()
        {
            if (parent != null) parent.localScale = Vector3.one;
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            ClearParent();

            float originX = -width * cellSize / 2f;
            float originZ = -height * cellSize / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float cordX = (float)x / width * scale + offsetX;
                    float cordY = (float)y / height * scale + offsetY;

                    float noise = Mathf.PerlinNoise(cordX, cordY);

                    GameObject prefab = noise < threshold ? floorPrefab : wallPrefab;
                    Vector3 pos = new Vector3(originX + x * cellSize, 0, originZ + y * cellSize);

                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent);

                    if (prefab == floorPrefab)
                    {
                        obj.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                        obj.transform.position = new Vector3(pos.x, 0, pos.z);
                    }
                    else if (prefab == wallPrefab)
                    {
                        float wallBottomY = wallHeight / 2f;
                        obj.transform.position = new Vector3(pos.x, wallBottomY, pos.z);
                        obj.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
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