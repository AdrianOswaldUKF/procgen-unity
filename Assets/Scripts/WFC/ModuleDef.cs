using UnityEngine;

[CreateAssetMenu(menuName = "WFC/ModuleDef")]
public class ModuleDef : ScriptableObject
{
    public GameObject prefab;
    public string[] ports = new string[6];

    public string GetPort(int dir)
    {
        if (ports == null || dir < 0 || dir >= ports.Length)
        {
            return string.Empty;
        }
        return ports[dir] ?? string.Empty;
    }
}