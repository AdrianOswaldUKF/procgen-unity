using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoise
{
    public class PnBiome : MonoBehaviour
    {
        public int width = 64;
        public int height = 64;
        public int scale = 8;
        public float offsetX;
        public float offsetY;

        public GameObject grassPrefab;
        public GameObject rockPrefab;
        public GameObject waterPrefab;
        public Transform parent;

        [ContextMenu("Generate")]
        public void Generate()
        {
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            ClearParent();
            GenerateLevelPart();
        }

        [ContextMenu("ClearParent")]
        public void ClearContext()
        {
            ClearParent();
        }

        void Start()
        {
            Telemetry.Instance?.RecordGenerationStart("PerlinNoiseBiome");
            Generate();
            Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseBiome");
        }

        void GenerateLevelPart()
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

                    GameObject prefab;
                    if (heightNoise > 0.6f) prefab = rockPrefab;
                    else if (moistureNoise > 0.7f) prefab = waterPrefab;
                    else prefab = grassPrefab;

                    Vector3 pos = new Vector3(originX + x, 0, originZ + y);
                    Instantiate(prefab, pos, Quaternion.identity, parent);
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