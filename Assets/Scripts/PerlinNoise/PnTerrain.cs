using UnityEngine;

namespace PerlinNoise
{
    public class PnTerrain : PnGenerator
    {
        [Header("Terrain")] 
        public GameObject terrainObject;
        public Terrain terrain;
        private TerrainData _terrainData;
        private int _heightRes;

        protected override string TelemetryName => "PerlinNoiseTerrain";

        private void SetupTerrainData()
        {
            if (terrain == null)
            {
                Debug.LogWarning("[PnTerrain] Terrain not assigned.");
                return;
            }
            _terrainData = terrain.terrainData;
            _heightRes = _terrainData.heightmapResolution;
            width = height = _heightRes;
        }
        
        [ContextMenu("Reset Terrain")]
        private void ResetTerrain()
        {
            if (_terrainData == null)
                return;
            
            float[,] heights = new float[_heightRes, _heightRes];
            _terrainData.SetHeights(0, 0, heights);
        }

        protected override void GenerateNoise()
        {
            if (terrain == null) 
                terrain = GetComponent<Terrain>();
            
            SetupTerrainData();
            terrainObject?.SetActive(true);
            ResetTerrain();
            
            float[,] heights = new float[_heightRes, _heightRes];
            for (int x = 0; x < _heightRes; x++)
            {
                for (int y = 0; y < _heightRes; y++)
                {
                    float xCoord = (float)x / _heightRes * scale + offsetX;
                    float yCoord = (float)y / _heightRes * scale + offsetY;
                    heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * heightMultiplier;
                }
            }
            _terrainData.SetHeights(0, 0, heights);
        }
    }
}