using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace LSystems
{
    public class LsCave : MonoBehaviour
    {
        [Header("L-System")] 
        public string axiom = "C";
        public int iterations = 4;
        public string[] rules = 
        {
            "C=F[+C][-C]FC",
            "F=F"
        };

        [Header("Cave")] 
        public float step = 4f;
        public float angle = 35f;
        public float tunnelWidth = 2.5f;
        public float tunnelHeight = 2f;
        public float yOffset = 1f;

        public GameObject tunnelPrefab;
        public Transform parent;

        public TMP_InputField axiomInput;
        public TMP_InputField iterationsInput;
        public TMP_InputField rulesInput;

        [ContextMenu("Generate")]
        public void Generate()
        {
            UpdateInputs();
            Telemetry.Instance?.RecordGenerationStart("LSystemCave");
            
            LSystem lsystem = new LSystem(rules);
            string result = lsystem.Expand(axiom, iterations);
            RenderCave(result);

            Telemetry.Instance?.RecordGenerationEnd("LSystemCave");
        }
        
        [ContextMenu("ClearParent")]
        public void ClearContext()
        {
            ClearParent();
        }

        void UpdateInputs()
        {
            if (axiomInput != null && axiomInput.text != "") axiom = axiomInput.text;
            if (iterationsInput != null && iterationsInput.text != "") iterations = int.Parse(iterationsInput.text);
            if (rulesInput != null && rulesInput.text != "") rules = rulesInput.text.Split(',');
        }

        void RenderCave(string commands)
        {
            ClearParent();
            Stack<LSystemState> stack = new Stack<LSystemState>();
            Vector3 pos = Vector3.zero + Vector3.up * yOffset;
            Quaternion rot = Quaternion.identity;
            Vector3 lastPos = pos;

            foreach (char cmd in commands)
            {
                switch (cmd)
                {
                    case 'F':
                        Vector3 nextPos = pos + rot * Vector3.forward * step;
                        CreateTunnel(lastPos, nextPos);
                        lastPos = nextPos;
                        pos = nextPos;
                        break;

                    case '+':
                        rot *= Quaternion.Euler(0, angle, 0);
                        break;

                    case '-':
                        rot *= Quaternion.Euler(0, -angle, 0);
                        break;

                    case '[':
                        stack.Push(new LSystemState(pos, rot));
                        break;

                    case ']':
                        if (stack.Count > 0)
                        {
                            LSystemState state = stack.Pop();
                            pos = state.pos;
                            rot = state.rot;
                            lastPos = pos;
                        }
                        break;
                }
            }
        }

        void CreateTunnel(Vector3 start, Vector3 end)
        {
            if (tunnelPrefab == null || parent == null) return;

            Vector3 midPoint = (start + end) * 0.5f;
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            
            Quaternion directionRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            
            Quaternion prefabRotation = tunnelPrefab.transform.rotation;
            
            Quaternion finalRotation = directionRotation * prefabRotation;
    
            GameObject tunnel = Instantiate(tunnelPrefab, midPoint, finalRotation, parent);
            
            tunnel.transform.localScale = new Vector3(tunnelWidth, tunnelHeight, distance);
        }


        void ClearParent()
        {
            if (parent == null) return;
            while (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}