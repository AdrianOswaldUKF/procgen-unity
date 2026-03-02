using UnityEngine;

public class PerlinNoiseTexture : MonoBehaviour
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
        _renderer =  GetComponent<Renderer>();
        _renderer.material.mainTexture = GenerateTexture();
    }
    
    void Start()
    {
        Telemetry.Instance?.RecordGenerationStart("PerlinNoiseTexture");
        Generate();
        Telemetry.Instance?.RecordGenerationEnd("PerlinNoiseTexture");
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
        float cordX = (float) x / width * scale + offsetX;
        float cordY = (float) y / height * scale + offsetY;
        
        float noise = Mathf.PerlinNoise(cordX, cordY);
        return new Color(noise, noise, noise);
    }
}
