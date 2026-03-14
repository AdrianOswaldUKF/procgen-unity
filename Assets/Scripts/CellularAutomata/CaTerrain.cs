using UnityEngine;

namespace CellularAutomata
{
    public class CaTerrain : CaGenerator
    {
        [Header("Terrain")]
        public GameObject terrainPrefab;
        public float terrainHeight = 1f;

        [Header("CA Settings")]
        public float fillProbability = 0.5f;
        public int iterations = 3;
        public int birthThreshold = 4;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaTerrain";

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

        protected override void PlacePrefabs()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y]) 
                        continue;
                    
                    Vector3 pos = GridToWorld(x, y) + Vector3.up * (terrainHeight * 0.5f);
                    GameObject go = Instantiate(terrainPrefab, pos, Quaternion.identity, parent);
                    go.transform.localScale = new Vector3(cellSize, terrainHeight, cellSize);
                }
            }
        }

        protected override void SetupUI()
        {
            seedInput.text = seed;
            fillProbabilitySlider.value = fillProbability;
            iterationsInput.text = iterations.ToString();
            birthThresholdInput.text = birthThreshold.ToString();
        }
    }
}