// CA kód inšpirácia od Sebastian Lague  
// https://github.com/SebLague/Procedural-Cave-Generation
// https://www.youtube.com/playlist?list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9 

using UnityEngine;

namespace CellularAutomata
{
    public class CaCave : CaGenerator
    {
        [Header("Cave")]
        public GameObject prefab;
        public float caveHeight = 15f;

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
                    if (!grid[x, y]) 
                        continue;
                    Vector3 basePos = GridToWorld(x, y);
                    Vector3 cavePos = basePos + Vector3.up * (caveHeight * 0.5f);
                    GameObject go = Instantiate(prefab, cavePos, Quaternion.identity, parent);
                    go.transform.localScale = new Vector3(cellSize, caveHeight, cellSize);
                }
            }
        }

        protected override void SetupUI()
        {
            if (widthInput != null) 
                widthInput.text = width.ToString();
            
            if (heightInput != null) 
                heightInput.text = height.ToString();
            
            if (cellSizeInput != null) 
                cellSizeInput.text = cellSize.ToString();
            
            if (seedInput != null) 
                seedInput.text = seed;
            
            if (fillProbabilitySlider != null) 
                fillProbabilitySlider.value = fillProbability;
            
            if (iterationsInput != null) 
                iterationsInput.text = iterations.ToString();
            
            if (birthThresholdInput != null) 
                birthThresholdInput.text = birthThreshold.ToString();
        }
    }
}