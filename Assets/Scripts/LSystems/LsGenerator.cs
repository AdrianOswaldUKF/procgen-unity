using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace LSystems
{
    public abstract class LsGenerator : MonoBehaviour
    {
        [Header("L-System")]
        public string axiom = "F";
        public int iterations = 3;
        public string[] rules = { "F=F" };

        [Header("Render")]
        public float step = 5f;
        public float angle = 45f;
        public float yOffset;

        [Header("UI")]
        public TMP_InputField axiomInput;
        public TMP_InputField iterationsInput;
        public TMP_InputField rulesInput;

        [Header("Output")]
        public Transform parent;

        protected abstract string TelemetryName { get; }
        protected abstract void RenderSegment(Vector3 start, Vector3 end);

        [ContextMenu("Generate")]
        public void Generate()
        {
            ReadUIInputs();
            Telemetry.Instance?.StartPCG(TelemetryName);
            
            LSystem lsystem = new LSystem(rules);
            string result = lsystem.Expand(axiom, iterations);
            LogLSystemMetrics(result);
            RenderLSystem(result);

            Telemetry.Instance?.EndPCG();
        }
        
        protected void LogLSystemMetrics(string commands)
        {
            int stringLength = commands.Length;
            int segmentCount = 0;
            foreach (char cmd in commands)
            {
                if (cmd == 'F' || cmd == 'R') segmentCount++;
            }
    
            Telemetry.Instance?.LogLSystem(
                stringLength, segmentCount, rules.Length, iterations
            );
        }
        
        [ContextMenu("Clear Parent")]
        public void ClearParentContext()
        {
            ClearParent();
        }

        protected virtual void ReadUIInputs()
        {
            if (!Application.isPlaying) return;
            
            if (axiomInput != null && axiomInput.text != "") axiom = axiomInput.text;
            if (iterationsInput != null && iterationsInput.text != "") iterations = int.Parse(iterationsInput.text);
            if (rulesInput != null && rulesInput.text != "") rules = rulesInput.text.Split(',');
        }

        private void RenderLSystem(string commands)
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
                    case 'R':
                        Vector3 nextPos = pos + rot * Vector3.forward * step;
                        RenderSegment(lastPos, nextPos);
                        lastPos = nextPos;
                        pos = nextPos;
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
                            lastPos = pos;
                        }
                        break;
                }
            }
        }

        private void ClearParent()
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

        protected virtual void Start()
        {
            SetupUI();
            Generate();
        }

        protected virtual void SetupUI()
        {
            if (axiomInput != null) 
                axiomInput.text = axiom;
            
            if (iterationsInput != null) 
                iterationsInput.text = iterations.ToString();
            
            if (rulesInput != null) 
                rulesInput.text = string.Join(",", rules);
        }
    }
}