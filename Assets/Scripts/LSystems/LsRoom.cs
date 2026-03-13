using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace LSystems
{
    public class LsRoom : MonoBehaviour
    {
        [Header("L-System")] 
        public string axiom = "FFFF";
        public int iterations = 2;
        public string[] rules =
        {
            "F=[+FF][-FF]FF[+FF][-FF]",
        };

        [Header("Room")] 
        public float step = 8f;
        public float angle = 90f;
        public GameObject roomPrefab;
        public float yOffset = 1f;

        public TMP_InputField axiomInput;
        public TMP_InputField iterationsInput;
        public TMP_InputField rulesInput;
        public Transform parent;

        [ContextMenu("Generate")]
        public void Generate()
        {
            UpdateInputs();
            Telemetry.Instance?.RecordGenerationStart("LSystemRoom");
            
            LSystem lsystem = new LSystem(rules);
            string result = lsystem.Expand(axiom, iterations);
            RenderRooms(result);

            Telemetry.Instance?.RecordGenerationEnd("LSystemRoom");
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

        void RenderRooms(string commands)
        {
            ClearParent();
            Stack<LSystemState> stack = new Stack<LSystemState>();
            Vector3 pos = Vector3.zero + Vector3.up * yOffset;
            Quaternion rot = Quaternion.identity;

            foreach (char cmd in commands)
            {
                switch (cmd)
                {
                    case 'F':
                    case 'R':
                        if (roomPrefab != null)
                        {
                            Vector3 roomPos = pos;
                            GameObject room = Instantiate(roomPrefab, roomPos, rot, parent);
                        }
                        pos += rot * Vector3.forward * step;
                        break;

                    case '+': rot *= Quaternion.Euler(0, angle, 0); break;
                    case '-': rot *= Quaternion.Euler(0, -angle, 0); break;
                    case '[': stack.Push(new LSystemState(pos, rot)); break;
                    case ']': 
                        if (stack.Count > 0)
                        {
                            LSystemState state = stack.Pop();
                            pos = state.pos;
                            rot = state.rot;
                        }
                        break;
                }
            }
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