using UnityEngine;

namespace CellularAutomata
{
    public class CaBiome : CaGenerator
    {
        [Header("Biome")]
        public GameObject biomeA;
        public GameObject biomeB;

        [Header("CA Settings")]
        public float fillProbability = 0.5f;
        public int iterations = 3;
        public int birthThreshold = 4;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaBiome";

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
                    Vector3 pos = GridToWorld(x, y);
                    GameObject prefab = grid[x, y] ? biomeA : biomeB;
                    if (prefab == null) continue;

                    GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);
                    go.transform.localScale = Vector3.one * cellSize;
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