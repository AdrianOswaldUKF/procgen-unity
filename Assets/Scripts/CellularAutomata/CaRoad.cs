using UnityEngine;

namespace CellularAutomata
{
    public class CaRoad : CaGenerator
    {
        [Header("Road")]
        public GameObject roadPrefab;

        [Header("CA Settings")]
        public float fillProbability = 0.3f;
        public int iterations = 4;
        public int birthThreshold = 2;
        public int deathThreshold = 6;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaRoad";

        private int DeathThreshold => deathThreshold;

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

        protected override void Step()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbors = CountNeighbors(x, y);
                    bool alive = grid[x, y];

                    if (alive && neighbors < DeathThreshold)
                        buffer[x, y] = false;
                    else if (!alive && neighbors >= DefaultBirthThreshold)
                        buffer[x, y] = true;
                    else
                        buffer[x, y] = alive;
                }
            }
            (grid, buffer) = (buffer, grid);
        }

        protected override void PlacePrefabs()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y] || roadPrefab == null) 
                        continue;
                    
                    Vector3 pos = GridToWorld(x, y);
                    GameObject road = Instantiate(roadPrefab, pos, Quaternion.identity, parent);
                    road.transform.localScale = Vector3.one * (cellSize / 10f);
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