using UnityEngine;

namespace CellularAutomata
{
    public class CaCave : CaGenerator
    {
        [Header("Cave")]
        public GameObject prefab;
        public float caveHeight = 2f;

        [Header("CA Settings")]
        public float fillProbability = 0.65f;
        public int iterations = 5;
        public int birthThreshold = 5;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaCave";

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
                    if (!grid[x, y]) continue;
                    Vector3 basePos = GridToWorld(x, y);
                    Vector3 cavePos = basePos + Vector3.up * (caveHeight * 0.5f);
                    GameObject go = Instantiate(prefab, cavePos, Quaternion.identity, parent);
                    go.transform.localScale = new Vector3(cellSize, caveHeight, cellSize);
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