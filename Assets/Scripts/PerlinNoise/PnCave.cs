using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoise
{
    public class PnCave : MonoBehaviour
    {
        [Header("Grid Size")] 
        public int width = 128;
        public int height = 128;

        [Header("Perlin Noise")] 
        public int scale = 2;
        public float threshold = 0.45f;
        public float offsetX;
        public float offsetY;

        [Header("Cave Size")] 
        public float cellSize = 2f;
        public float wallHeight = 3f;

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
            GenerateCave();
        }

        [ContextMenu("ClearParent")]
        public void ClearContext()
        {
            ClearParent();
        }

        void Start()
        {
            Telemetry.Instance?.RecordGenerationStart("PerlinNoiseCave");
            Generate();
            Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseCave");
        }

        void GenerateCave()
        {
            float originX = -width * cellSize / 2f;
            float originZ = -height * cellSize / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float cordX = (float)x / width * scale + offsetX;
                    float cordY = (float)y / height * scale + offsetY;

                    float noise = Mathf.PerlinNoise(cordX, cordY);
                    bool isCave = noise < threshold;

                    GameObject prefab = isCave ? floorPrefab : wallPrefab;
                    Vector3 pos = new Vector3(originX + x * cellSize, 0, originZ + y * cellSize);

                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent);

                    if (isCave)
                    {
                        obj.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
                    }
                    else
                    {
                        obj.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
                        obj.transform.position = new Vector3(pos.x, wallHeight / 2f, pos.z);
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