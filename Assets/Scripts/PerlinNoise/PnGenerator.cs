using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerlinNoise
{
    public abstract class PnGenerator : MonoBehaviour
    {
        [Header("Grid")] 
        public int width = 64;
        public int height = 64;

        [Header("Perlin Noise")] 
        public int scale = 8;
        public float offsetX;
        public float offsetY;
        public float heightMultiplier = 1;
        public string seed;
        
        [Header("UI")] 
        public TMP_InputField seedInput;
        public TMP_InputField scaleInput;
        public TMP_InputField heightMultiplierInput;

        [Header("Output")] 
        public Transform parent;

        protected abstract string TelemetryName { get; }
        protected abstract void GenerateGrid();

        [ContextMenu("Generate")]
        public void Generate()
        {
            ReadUIInputs();
            
            if (!string.IsNullOrEmpty(seed))
                Random.InitState(seed.GetHashCode());
            
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            
            ClearParent();
            Telemetry.Instance?.StartPCG(TelemetryName);
            GenerateGrid();
            
            LogPerlinMetrics();
            Telemetry.Instance?.EndPCG();
        }
        
        private void ReadUIInputs()
        {
            if (Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(seedInput.text))
                    seed = seedInput.text;

                if (!string.IsNullOrEmpty(scaleInput.text))
                    scale = int.Parse(scaleInput.text);

                if (!string.IsNullOrEmpty(heightMultiplierInput.text))
                    heightMultiplier = float.Parse(heightMultiplierInput.text);
            }
        }
        
        protected void LogPerlinMetrics()
        {
            float sum = 0f, sumSq = 0f, count = 0f;
    
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float noiseValue = Mathf.PerlinNoise(
                        (float)x / width * scale + offsetX,
                        (float)y / height * scale + offsetY
                    );
                    sum += noiseValue;
                    sumSq += noiseValue * noiseValue;
                    count++;
                }
            }
    
            float avg = count > 0 ? sum / count : 0f;
            float stdDev = count > 0 ? Mathf.Sqrt((sumSq / count) - (avg * avg)) : 0f;
            float contrast = count > 0 ? stdDev / avg : 0f;
    
            Telemetry.Instance.LogPerlin(avg, stdDev, contrast, scale);
        }

        [ContextMenu("Clear Parent")]
        public void ClearParentContext()
        {
            ClearParent();
        }

        protected void ClearParent()
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

        protected virtual void Start()
        {
            if (Application.isPlaying)
            {
                seedInput.text = seed;
                scaleInput.text = scale.ToString();
                heightMultiplierInput.text = heightMultiplier.ToString();
            }
        }
    }
}