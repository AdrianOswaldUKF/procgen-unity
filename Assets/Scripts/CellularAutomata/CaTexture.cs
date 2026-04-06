using UnityEngine;

namespace CellularAutomata
{
    public class CaTexture : CaGenerator
    {
        [Header("Texture")]
        public Renderer targetRenderer;
        public Color aliveColor = Color.black;
        public Color deadColor = Color.white;

        [Header("CA Settings")]
        public float fillProbability = 0.65f;
        public int iterations = 5;
        public int birthThreshold = 5;

        private Texture2D _texture;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaTexture";

        protected override void ReadUIInputs()
        {
            base.ReadUIInputs();
            if (Application.isPlaying)
            {
                fillProbability = fillProbabilitySlider?.value ?? fillProbability;
                
                if (iterationsInput != null && iterationsInput.text != "") 
                    iterations = int.Parse(iterationsInput.text);
                
                if (birthThresholdInput != null && birthThresholdInput.text != "") 
                    birthThreshold = int.Parse(birthThresholdInput.text);
            }
        }

        protected override void InitializeGrid()
        {
            base.InitializeGrid();
            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Point;
            _texture.wrapMode = TextureWrapMode.Clamp;
            
            if (targetRenderer != null)
            {
                Material mat = new Material(targetRenderer.sharedMaterial);
                mat.mainTexture = _texture;
                targetRenderer.sharedMaterial = mat;
            }
        }

        protected override void PlacePrefabs() { }

        protected override void ApplyIterations()
        {
            base.ApplyIterations();
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _texture.SetPixel(x, y, grid[x, y] ? aliveColor : deadColor);
                }
            }
            _texture.Apply();
        }
    }
}