using UnityEngine;

namespace LSystems
{
    public struct LSystemState
    {
        public Vector3 pos;
        public Quaternion rot;

        public LSystemState(Vector3 p, Quaternion r)
        {
            pos = p;
            rot = r;
        }
    }
}