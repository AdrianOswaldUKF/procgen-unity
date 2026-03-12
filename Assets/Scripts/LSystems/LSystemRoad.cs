using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LSystemRoad : MonoBehaviour
{
    public string axiom = "FFFF";
    public int iterations = 1;
    public string[] rules = { "F=[+FF][-FF]FF[+FF][-FF]" };
    
    public TMP_InputField axiomInput;
    public TMP_InputField iterationsInput;
    public TMP_InputField rulesInput;
    
    public float step = 3f;
    public float angle = 90f;
    public float roadWidth = 1.5f;  
    public float roadHeight = 0.02f;
    public float yOffset = 0f;
    
    public GameObject roadPrefab;
    public Transform parent;
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        if (axiomInput.text != "")
        {
            axiom = axiomInput.text;
        }

        if (iterationsInput.text != "")
        {
            iterations = int.Parse(iterationsInput.text);
        }

        if (rulesInput.text != "")
        {
            rules = rulesInput.text.Split(',');
        }
        
        Telemetry.Instance?.RecordGenerationStart("LSystemRoad");
        LSystem expander = new LSystem(rules);
        string result = expander.Expand(axiom, iterations);
        
        RenderRoad(result);
        Telemetry.Instance?.RecordGenerationEnd("LSystemRoad");
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
    
    void RenderRoad(string commands)
    {
        ClearParent();
        
        Stack<LSystemState> stack = new Stack<LSystemState>();
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        Vector3 lastPos = pos;

        foreach (char command in commands)
        {
            switch (command)
            {
                case 'F':
                    Vector3 nextPos = pos + rot * Vector3.forward * step;
                    CreateRoad(lastPos, nextPos);
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
    
    void CreateRoad(Vector3 start, Vector3 end)
    {
        if (roadPrefab == null || parent == null) return;
        
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        if (distance < 0.01f) return;
        
        Vector3 midPoint = (start + end) * 0.5f;
        Quaternion roadRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
        
        GameObject road = Instantiate(roadPrefab, midPoint, roadRot, parent);

        Vector3 meshSize = Vector3.one;
        Vector3 meshBaseScale = Vector3.one;

        MeshFilter mf = roadPrefab.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            meshSize = mf.sharedMesh.bounds.size;

            Transform t = mf.transform;
            Vector3 scaleProduct = Vector3.one;

            while (t != null && t != roadPrefab.transform)
            {
                scaleProduct = Vector3.Scale(scaleProduct, t.localScale);
                t = t.parent;
            }

            meshBaseScale = Vector3.Scale(scaleProduct, roadPrefab.transform.localScale);
        }

        float meshX = Mathf.Max(meshSize.x * meshBaseScale.x, 0.000001f);
        float meshY = Mathf.Max(meshSize.y * meshBaseScale.y, 0.000001f);
        float meshZ = Mathf.Max(meshSize.z * meshBaseScale.z, 0.000001f);

        Vector3 scale = new Vector3(
            roadWidth / meshX,
            roadHeight / meshY,
            distance / meshZ
        );

        road.transform.localScale = scale;
        road.transform.position = new Vector3(midPoint.x, yOffset + roadHeight * 0.5f, midPoint.z);
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