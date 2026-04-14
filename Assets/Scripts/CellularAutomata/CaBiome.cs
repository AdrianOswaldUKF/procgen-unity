// CA kód inšpirácia od Sebastian Lague  
// https://github.com/SebLague/Procedural-Cave-Generation
// https://www.youtube.com/playlist?list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9 

using UnityEngine;

namespace CellularAutomata
{
    public class CaBiome : CaGenerator
    {
        [Header("Biome")]
        public Terrain terrain;
        private TerrainData _terrainData;
        private int _heightRes;
        private int _alphaRes;

        [Header("CA Settings")]
        public float fillProbability = 0.4f;
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
            if (terrain == null)
            {
                Debug.LogWarning("[CaBiome] Terrain not assigned.");
                return;
            }

            SetupTerrainData();

            bool[,] waterGrid = new bool[width, height];
            bool[,] waterBuffer = new bool[width, height];

            string waterSeed = seed + "_water";
            Random.InitState(waterSeed.GetHashCode());

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    waterGrid[x, y] = Random.value < DefaultFillProbability;
                }
            }

            for (int i = 0; i < DefaultIterations; i++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int neighbors = CountWaterNeighbors(x, y, waterGrid);
                        waterBuffer[x, y] = neighbors >= DefaultBirthThreshold;
                    }
                }
                (waterGrid, waterBuffer) = (waterBuffer, waterGrid);
            }

            float[,,] alphaMaps = new float[_alphaRes, _alphaRes, 3];

            for (int ax = 0; ax < _alphaRes; ax++)
            {
                for (int ay = 0; ay < _alphaRes; ay++)
                {
                    int gx = Mathf.Clamp(Mathf.FloorToInt((float)ax / _alphaRes * width), 0, width - 1);
                    int gy = Mathf.Clamp(Mathf.FloorToInt((float)ay / _alphaRes * height), 0, height - 1);

                    alphaMaps[ay, ax, 0] = 0f;
                    alphaMaps[ay, ax, 1] = 0f;
                    alphaMaps[ay, ax, 2] = 0f;

                    if (grid[gx, gy])
                        alphaMaps[ay, ax, 1] = 1f;
                    else if (waterGrid[gx, gy])
                        alphaMaps[ay, ax, 2] = 1f;
                    else
                        alphaMaps[ay, ax, 0] = 1f;
                }
            }

            _terrainData.SetAlphamaps(0, 0, alphaMaps);
        }
        
        private int CountWaterNeighbors(int cx, int cy, bool[,] g)
        {
            int count = 0;
            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0) continue;
                    int nx = cx + ox;
                    int ny = cy + oy;
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height && g[nx, ny])
                        count++;
                }
            }
            return count;
        }
        
        [ContextMenu("Reset Terrain")]
        private void ResetTerrain()
        {
            if (_terrainData == null)
                return;

            float[,] heights = new float[_heightRes, _heightRes];
            _terrainData.SetHeights(0, 0, heights);
        }
        
        private void SetupTerrainData()
        {
            if (terrain == null)
            {
                Debug.LogWarning("[CaTerrain] Terrain not assigned.");
                return;
            }
            _terrainData = terrain.terrainData;
            _heightRes = _terrainData.heightmapResolution;
            _alphaRes = _terrainData.alphamapResolution;
            
            _terrainData.size = new Vector3(width, _terrainData.size.y, height);
            terrain.transform.position = new Vector3(-width / 2f, terrain.transform.position.y, -height / 2f);
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