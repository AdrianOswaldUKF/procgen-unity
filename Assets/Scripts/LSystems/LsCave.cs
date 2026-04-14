// Ls kód inšpirácia od eleonora
// https://www.youtube.com/watch?v=Sf6k6kvpRu4&

using UnityEngine;

namespace LSystems
{
    public class LsCave : LsGenerator
    {
        [Header("Cave Settings")]
        public GameObject tunnelPrefab;
        public float tunnelWidth = 2.5f;
        public float tunnelHeight = 2f;

        protected override string TelemetryName => "LSystemCave";

        protected override void RenderSegment(Vector3 start, Vector3 end)
        {
            if (tunnelPrefab == null || parent == null) 
                return;

            Vector3 midPoint = (start + end) * 0.5f;
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            
            Quaternion directionRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Quaternion prefabRotation = tunnelPrefab.transform.rotation;
            Quaternion finalRotation = directionRotation * prefabRotation;
    
            GameObject tunnel = Instantiate(tunnelPrefab, midPoint, finalRotation, parent);
            tunnel.transform.localScale = new Vector3(tunnelWidth, tunnelHeight, distance);
        }
    }
}