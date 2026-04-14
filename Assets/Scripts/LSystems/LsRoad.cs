// Ls kód inšpirácia od eleonora
// https://www.youtube.com/watch?v=Sf6k6kvpRu4&

using UnityEngine;

namespace LSystems
{
    public class LsRoad : LsGenerator
    {
        [Header("Road Settings")]
        public GameObject roadPrefab;
        public float roadWidth = 1.5f;
        public float roadHeight = 0.02f;

        protected override string TelemetryName => "LSystemRoad";

        protected override void RenderSegment(Vector3 start, Vector3 end)
        {
            if (roadPrefab == null || parent == null) 
                return;
    
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            if (distance < 0.01f) 
                return;

            Vector3 midPoint = (start + end) * 0.5f;
            Quaternion roadRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
            GameObject road = Instantiate(roadPrefab, midPoint, roadRot, parent);

            MeshFilter mf = road.GetComponentInChildren<MeshFilter>();
            Vector3 meshSize = Vector3.one;

            if (mf != null && mf.sharedMesh != null)
                meshSize = mf.sharedMesh.bounds.size;

            float meshX = Mathf.Max(meshSize.x, 0.0001f);
            float meshZ = Mathf.Max(meshSize.z, 0.0001f);

            float scaleX = roadWidth / meshX;
            float scaleZ = distance / meshZ;

            Vector3 baseScale = road.transform.localScale;

            road.transform.localScale = new Vector3(
                baseScale.x * scaleX,
                baseScale.y,
                baseScale.z * scaleZ
            );

            road.transform.position = new Vector3(midPoint.x, yOffset + roadHeight * 0.5f, midPoint.z);
        }
    }
}