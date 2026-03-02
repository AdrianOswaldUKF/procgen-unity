using System.Linq;
using TMPro;
using UnityEngine;

public class LSystemTreeGenerator : MonoBehaviour
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
        {
            axiom =  axiomInput.text;
        }

        if (iterationsInput.text != "")
        {
            iterations = int.Parse(iterationsInput.text);
        }

        if (rulesInput.text != "")
        {
            rules = rulesInput.text.Split(',');
        }
        Telemetry.Instance?.RecordGenerationStart("LSystemTreeGeneration");
        LSystem expander = new LSystem(rules);
        string result = expander.Expand(axiom, iterations);

        GetComponent<LSystemTreeRenderer>().Render(result);
        Telemetry.Instance?.RecordGenerationEnd("LSystemTreeGeneration");
    }

    void Start()
    {
        axiomInput.text = axiom;
        iterationsInput.text = iterations.ToString();
        foreach (string str in rules)
        {
            rulesInput.text += str;
            if (str == rules.Last())
            {
                return;
            }
            rulesInput.text += ",";
        }
        Generate();
    }
}