using UnityEngine;

namespace PerlinNoise
{
    public class PnTexture : PnGenerator
    {
        private Renderer _renderer;

        protected override string TelemetryName => "PerlinNoiseTexture";

        protected override void GenerateNoise()
        {
            _renderer = GetComponent<Renderer>();
            _renderer.material.mainTexture = GenerateTexture();
        }

        Texture2D GenerateTexture()
        {
            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, GenerateColor(x, y));
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