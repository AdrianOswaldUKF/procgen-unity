using UnityEngine;

namespace LSystems
{
    public class LsRoom : LsGenerator
    {
        [Header("Room Settings")]
        public GameObject roomPrefab;

        protected override string TelemetryName => "LSystemRoom";

        protected override void RenderSegment(Vector3 start, Vector3 end)
        {
            if (roomPrefab == null) 
                return;
            
            Vector3 pos = start;
            Instantiate(roomPrefab, pos, Quaternion.identity, parent);
        }
    }
}