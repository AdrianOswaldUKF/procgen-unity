using UnityEngine;

namespace PerlinNoise
{
    public class PnTexture : MonoBehaviour
    {
        public int width;

        public int height;

        public int scale;

        public float offsetX;
        public float offsetY;

        private Renderer _renderer;

        [ContextMenu("Generate")]
        public void Generate()
        {
            offsetX = Random.Range(0, 99999);
            offsetY = Random.Range(0, 99999);
            _renderer = GetComponent<Renderer>();
            _renderer.material.mainTexture = GenerateTexture();
            LogPerlinMetrics();
        }
        
        private void LogPerlinMetrics()
        {
            float sum = 0f, sumSq = 0f, count = 0f;
            Texture2D texture = _renderer.material.mainTexture as Texture2D;
    
            if (texture == null) return;
    
            Color[] pixels = texture.GetPixels();
            foreach (Color pixel in pixels)
            {
                float val = pixel.grayscale;
                sum += val;
                sumSq += val * val;
                count++;
            }
    
            float avg = count > 0 ? sum / count : 0f;
            float stdDev = count > 0 ? Mathf.Sqrt((sumSq / count) - (avg * avg)) : 0f;
            float contrast = count > 0 ? stdDev / avg : 0f;
    
            Metrics.Instance?.LogPerlin(avg, stdDev, contrast, scale);
        }

        void Start()
        {
            Metrics.Instance?.StartPcg("PerlinNoiseTexture");
            Generate();
            Metrics.Instance?.EndPcg();
        }

        Texture2D GenerateTexture()
        {
            Texture2D texture = new Texture2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = GenerateColor(x, y);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        Color GenerateColor(int x, int y)
        {
            float cordX = (float)x / width * scale + offsetX;
            float cordY = (float)y / height * scale + offsetY;

            float noise = Mathf.PerlinNoise(cordX, cordY);
            return new Color(noise, noise, noise);
        }
    }
}