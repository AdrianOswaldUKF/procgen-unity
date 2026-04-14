// Ls kód inšpirácia od eleonora
// https://www.youtube.com/watch?v=Sf6k6kvpRu4&

using UnityEngine;

namespace LSystems
{
    public class LsDungeon : LsGenerator
    {
        [Header("Dungeon Settings")]
        public GameObject wallPrefab;
        public float wallWidth = 0.8f;
        public float wallHeight = 4f;

        protected override string TelemetryName => "LSystemDungeon";

        protected override void RenderSegment(Vector3 start, Vector3 end)
        {
            if (wallPrefab == null) 
                return;
            
            Vector3 midPoint = (start + end) * 0.5f;
            GameObject wall = Instantiate(wallPrefab, midPoint, Quaternion.LookRotation(end - start), parent);
            float distance = Vector3.Distance(start, end);
            wall.transform.localScale = new Vector3(wallWidth, wallHeight, distance);
        }
    }
}