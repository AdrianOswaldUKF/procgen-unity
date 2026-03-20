using UnityEngine;

namespace LSystems
{
    public class LsRoom : LsGenerator
    {
        [Header("Room Settings")]
        public GameObject roomPrefab;
        public float roomWidth = 5f;
        public float roomHeight = 2f;

        protected override string TelemetryName => "LSystemRoom";

        protected override void RenderSegment(Vector3 start, Vector3 end)
        {
            if (roomPrefab == null || parent == null) 
                return;

            Vector3 direction = end - start;
            float distance = direction.magnitude;
            if (distance < 0.01f) 
                return;

            Vector3 midPoint = (start + end) * 0.5f;
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            GameObject room = Instantiate(roomPrefab, midPoint, rotation, parent);
            room.transform.localScale = new Vector3(roomWidth, roomHeight, distance);
        }
    }
}