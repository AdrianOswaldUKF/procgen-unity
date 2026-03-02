using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class LSystemTreeRenderer : MonoBehaviour
{
    [Header("L-System")]
    public float step = 0.4f;
    public float angle = 25f;

    [Header("Extrude")]
    public Mesh extrudeMesh;
    public Material extrudeMaterial;
    public float extrudeRadius = 0.05f;
    public float segmentsPerUnit = 4f;

    public void Render(string commands)
    {
        if (string.IsNullOrEmpty(commands))
        {
            return;
        }
        
        Transform old = transform.Find("Tree_Generated");
        if (old != null)
        {
            DestroyImmediate(old.gameObject);
        }
        
        GameObject tree = new GameObject("Tree_Generated");
        tree.transform.SetParent(transform, false);

        SplineContainer container = tree.AddComponent<SplineContainer>();
        SplineExtrude extrude = tree.AddComponent<SplineExtrude>();
        extrude.Container = container;
        extrude.targetMesh = extrudeMesh;
        extrude.Radius = extrudeRadius;
        extrude.SegmentsPerUnit = segmentsPerUnit;
        if (extrudeMaterial != null)
        {
            extrude.GetComponent<Renderer>().sharedMaterial = extrudeMaterial;
        }

        Stack<LSystemState> stack = new Stack<LSystemState>();
        List<Vector3> points = new List<Vector3>();

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        void Flush()
        {
            if (points.Count < 2)
            {
                points.Clear(); return;
            }

            Spline spline = container.AddSpline();
            BezierKnot[] knots = new BezierKnot[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                knots[i] = new BezierKnot(points[i]);
            }

            spline.Knots = knots;
            points.Clear();
        }

        points.Add(pos);

        foreach (char c in commands)
        {
            switch (c)
            {
                case 'F':
                    pos += rot * Vector3.forward * step;
                    points.Add(pos);
                    break;

                case 'l': 
                    rot *= Quaternion.Euler(0, angle, 0); 
                    break;
                case 'r': 
                    rot *= Quaternion.Euler(0, -angle, 0); 
                    break;

                case '[':
                    Flush();
                    stack.Push(new LSystemState(pos, rot));
                    points.Add(pos);
                    break;

                case ']':
                    Flush();
                    if (stack.Count > 0)
                    {
                        LSystemState s = stack.Pop();
                        pos = s.pos;
                        rot = s.rot;
                        points.Add(pos);
                    }
                    break;
            }
        }

        Flush();
    }
}
