using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LSystems
{
    public class LsTreeGen : MonoBehaviour
    {
        [Header("L-System")]
        public string axiom = "FB";
        public int iterations = 4;
        public string[] rules;
        
        [Header("UI")]
        public TMP_InputField axiomInput;
        public TMP_InputField iterationsInput;
        public TMP_InputField rulesInput;

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (axiomInput.text != "")
                axiom = axiomInput.text;

            if (iterationsInput.text != "")
                iterations = int.Parse(iterationsInput.text);

            if (rulesInput.text != "")
                rules = rulesInput.text.Split(',');

            Metrics.Instance?.StartPcg("LSystemTree");
            LSystem expander = new LSystem(rules);
            string result = expander.Expand(axiom, iterations);
            LogLSystemMetrics(result);

            GetComponent<LsTreeRender>().Render(result);
            Metrics.Instance?.EndPcg();
            ResetCylinderMesh();
        }
        
        protected void LogLSystemMetrics(string commands)
        {
            int stringLength = commands.Length;
            int segmentCount = 0;
            foreach (char cmd in commands)
            {
                if (cmd == 'F' || cmd == 'R') segmentCount++;
            }
    
            Metrics.Instance?.LogLSystem(
                stringLength, segmentCount, rules.Length, iterations
            );
        }

        void Start()
        {
            SetupUI();
        }
        
        private void SetupUI()
        {
            if (axiomInput != null) 
                axiomInput.text = axiom;
            
            if (iterationsInput != null) 
                iterationsInput.text = iterations.ToString();
            
            if (rulesInput != null) 
                rulesInput.text = string.Join(",", rules);
        }
        
        void ResetCylinderMesh()
        {
            EditorUtility.SetDirty(gameObject);
            Resources.UnloadUnusedAssets();
        }
    }
}