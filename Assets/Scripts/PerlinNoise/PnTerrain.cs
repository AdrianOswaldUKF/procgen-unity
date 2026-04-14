// Pn kód inšpirácia od DVS Devs(Dan Violet Sagmiller)
// https://www.youtube.com/watch?v=1qSjCu8av7Q
// Unity Dokumentácia: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html

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
            
            _terrainData.size = new Vector3(width, _terrainData.size.y, height);
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
                    float xCoord = (float)x / width * scale + offsetX;
                    float yCoord = (float)y / height * scale + offsetY;
                    heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * heightMultiplier;
                }
            }
            _terrainData.SetHeights(0, 0, heights);
            terrain.transform.position = new Vector3(-width / 2f, terrain.transform.position.y, -height / 2f);
        }
    }
}