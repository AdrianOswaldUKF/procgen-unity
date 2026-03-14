using UnityEngine;

namespace CellularAutomata
{
    public class CaDungeon : CaGenerator
    {
        [Header("Dungeon")]
        public GameObject wallPrefab;
        public GameObject floorPrefab;
        public float wallHeight = 2f;

        [Header("CA Settings")]
        public float fillProbability = 0.45f;
        public int iterations = 4;
        public int birthThreshold = 4;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaDungeon";

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
                    Vector3 basePos = GridToWorld(x, y);
                    if (grid[x, y] && wallPrefab != null)
                    {
                        Vector3 wallPos = basePos + Vector3.up * (wallHeight * 0.5f);
                        GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, parent);
                        wall.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
                    }
                    else if (floorPrefab != null)
                    {
                        GameObject floor = Instantiate(floorPrefab, basePos, Quaternion.identity, parent);
                        floor.transform.localScale = Vector3.one * cellSize;
                    }
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