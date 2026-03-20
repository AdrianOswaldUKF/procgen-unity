using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LSystems
{
    public class LsTreeGen : MonoBehaviour
    {
        public string axiom = "FB";
        public int iterations = 4;
        public string[] rules;

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

            Metrics.Instance?.StartPCG("LSystemTree");
            LSystem expander = new LSystem(rules);
            string result = expander.Expand(axiom, iterations);
            LogLSystemMetrics(result);

            GetComponent<LsTreeRender>().Render(result);
            Metrics.Instance?.EndPCG();
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
            axiomInput.text = axiom;
            iterationsInput.text = iterations.ToString();
            
            foreach (string str in rules)
            {
                rulesInput.text += str;
                if (str == rules.Last())
                    return;

                rulesInput.text += ",";
            }

            Generate();
        }
        
        void ResetCylinderMesh()
        {
            EditorUtility.SetDirty(gameObject);
            Resources.UnloadUnusedAssets();
        }
    }
}