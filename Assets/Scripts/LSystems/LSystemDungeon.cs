using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LSystemDungeon : MonoBehaviour
{
    [Header("L-System")]
    public string axiom = "F";
    public int iterations = 4;
    public string[] rules = { 
        "F=F[+F][-F]F",
        "X=F[+X][-X]X",
        "A=F[+A][-A]A",
        "B=F[+B][-B]B"
    };

    [Header("Dungeon")]
    public float step = 6f;
    public float angle = 45f;
    public float wallWidth = 0.8f;
    public float wallHeight = 4f;
    public float yOffset = 2f;

    public GameObject wallPrefab;
    
    public TMP_InputField axiomInput;
    public TMP_InputField iterationsInput;
    public TMP_InputField rulesInput;
    
    public Transform parent;
    
    [ContextMenu("Generate")]
    public void Generate()
    {
        UpdateInputs();
        Telemetry.Instance?.RecordGenerationStart("LSystemDungeon");
        LSystem lsystem = new LSystem(rules);
        string result = lsystem.Expand(axiom, iterations);
        RenderDungeon(result);
        
        Telemetry.Instance?.RecordGenerationEnd("LSystemDungeon");
    }
    
    void Start()
    {
        UpdateInputs();
        Generate();
    }
    
    void UpdateInputs()
    {
        if (axiomInput?.text != "") axiom = axiomInput.text;
        if (iterationsInput?.text != "") iterations = int.Parse(iterationsInput.text);
        if (rulesInput?.text != "") rules = rulesInput.text.Split(',');
    }
    
    void RenderDungeon(string commands)
    {
        ClearParent();
        
        Stack<LSystemState> stack = new Stack<LSystemState>();
        Vector3 pos = Vector3.zero + Vector3.up * yOffset;
        Quaternion rot = Quaternion.identity;
        Vector3 lastPos = pos;
        
        int wallCount = 0;
        
        foreach (char cmd in commands)
        {
            switch (cmd)
            {
                case 'F':
                    Vector3 nextPos = pos + rot * Vector3.forward * step;
                    CreateWall(lastPos, nextPos);
                    wallCount++;
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
                    }
                    break;
            }
        }
    }
    
    void CreateWall(Vector3 start, Vector3 end)
    {
        if (wallPrefab == null)
        {
            return;
        }
        if (parent == null)
        {
            return;
        }
        
        Vector3 midPoint = (start + end) * 0.5f;
        GameObject wall = Instantiate(wallPrefab, midPoint, Quaternion.LookRotation(end - start), parent);
        float distance = Vector3.Distance(start, end);
        wall.transform.localScale = new Vector3(wallWidth, wallHeight, distance);
    }
    
    void ClearParent()
    {
        if (parent == null) 
        {
            return;
        }
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
