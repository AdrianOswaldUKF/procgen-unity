using TMPro;
using UnityEngine;

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
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;
        public TMP_InputField seedInput;
        public TMP_InputField scaleInput;
        public TMP_InputField heightMultiplierInput;

        [Header("Output")] 
        public Transform parent;

        protected abstract string TelemetryName { get; }
        protected abstract void GenerateNoise();

        [ContextMenu("Generate")]
        public void Generate()
        {
            ReadUIInputs();
            
            if (!string.IsNullOrEmpty(seed))
                Random.InitState(seed.GetHashCode());
            
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            
            ClearParent();
            Metrics.Instance?.StartPcg(TelemetryName);
            GenerateNoise();
            
            LogPerlinMetrics();
            Metrics.Instance?.EndPcg();
        }
        
        private void ReadUIInputs()
        {
            if (!Application.isPlaying) 
                return;
            
            if (!string.IsNullOrEmpty(widthInput.text))
                width = int.Parse(widthInput.text);
            
            if (!string.IsNullOrEmpty(heightInput.text))
                height = int.Parse(heightInput.text);
    
            seed = seedInput?.text ?? "";
    
            if (!string.IsNullOrEmpty(scaleInput.text)) 
                scale = int.Parse(scaleInput.text);
            
            if (!string.IsNullOrEmpty(heightMultiplierInput.text)) 
                heightMultiplier = float.Parse(heightMultiplierInput.text);
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
    
            Metrics.Instance?.LogPerlin(avg, stdDev, contrast, scale);
        }

        [ContextMenu("Clear Parent")] 
        public void ClearParentContext()
        {
            ClearParent();
        }

        private void ClearParent()
        {
            if (parent == null)
                return;
            
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        
        private void SetupUI()
        {
            if (widthInput != null) 
                widthInput.text = width.ToString();
            
            if (heightInput != null) 
                heightInput.text = height.ToString();
            
            if (seedInput != null)
                seedInput.text = seed;
            
            if (scaleInput != null) 
                scaleInput.text = scale.ToString();
            
            if (heightMultiplierInput != null) 
                heightMultiplierInput.text = heightMultiplier.ToString();
        }

        protected virtual void Start()
        {
            SetupUI();
        }
    }
}