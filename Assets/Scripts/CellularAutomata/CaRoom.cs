// CA kód inšpirácia od Sebastian Lague  
// https://github.com/SebLague/Procedural-Cave-Generation
// https://www.youtube.com/playlist?list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9 

using UnityEngine;

namespace CellularAutomata
{
    public class CaRoom : CaGenerator
    {
        [Header("Room")]
        public GameObject roomPrefab;

        [Header("CA Settings")]
        public float fillProbability = 0.4f;
        public int iterations = 4;
        public int birthThreshold = 4;

        protected override float DefaultFillProbability => fillProbability;
        protected override int DefaultIterations => iterations;
        protected override int DefaultBirthThreshold => birthThreshold;
        protected override string TelemetryName => "CaRoom";

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
                    
                    Vector3 pos = GridToWorld(x, y);
                    GameObject go = Instantiate(roomPrefab, pos, Quaternion.identity, parent);
                    go.transform.localScale = Vector3.one * cellSize;
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