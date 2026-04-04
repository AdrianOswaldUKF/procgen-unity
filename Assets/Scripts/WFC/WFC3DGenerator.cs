using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WFC
{
    public class WFC3DGenerator : MonoBehaviour
    {
        [Header("Grid")] public int width = 8;
        public int height = 8;
        public int depth = 4;
        public float cellSize = 1f;

        [Header("Modules")] public ModuleDef[] modules;

        [Header("Random")] public string seed;
        public int maxRestarts = 20;

        public TMP_InputField seedInput;
        public TMP_InputField maxRestartsInput;

        [Header("Output")] public Transform parentForPrefabs;

        [ContextMenu("Generate")]
        public void GenerateWFC()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;
            
            seed = seedInput?.text ?? "";

            if (!string.IsNullOrEmpty(maxRestartsInput.text))
                maxRestarts = int.Parse(maxRestartsInput.text);

            ClearPrefabs();
            Generate();
        }

        [ContextMenu("Clear Parent")]
        public void ClearParentContext()
        {
            if (!Metrics.Instance.CanGenerate)
                return;
            
            ClearPrefabs();
        }

        private void Generate()
        {
            Metrics.Instance?.StartPcg("WFC3DGeneration");

            if (modules == null || modules.Length == 0)
            {
                Debug.LogError("[WFC3D] modules not assigned.");
                Metrics.Instance?.EndPcg();
                return;
            }

            int attempts = 0;
            bool ok = false;
            while (attempts < maxRestarts && !ok)
            {
                attempts++;
                InitRandom();
                ok = TryRun(out int[,,] result, attempts);
                if (!ok)
                {
                    Debug.LogWarning("[WFC3D] attempt " + attempts + " failed, retrying...");
                }
                else
                {
                    Debug.Log("[WFC3D] success on attempt " + attempts);
                    InstantiateResult(result);
                }
            }

            if (!ok)
            {
                Debug.LogError("[WFC3D] generation failed after maxRestarts=" + maxRestarts);
            }

            Metrics.Instance?.EndPcg();
        }

        private void LogWFCMetrics(int attempts, int propagationSteps, HashSet<int>[,,] possible)
        {
            float avgEntropy = 0f;
            int totalCells = width * depth * height;
            int cellCount = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        avgEntropy += possible[x, y, z].Count;
                        cellCount++;
                    }
                }
            }

            avgEntropy = cellCount > 0 ? avgEntropy / cellCount : 0f;
            float moduleVariety = modules.Length > 0 ? cellCount / (float)(modules.Length * totalCells) : 0f;

            Metrics.Instance?.LogWfc(attempts, propagationSteps, avgEntropy, moduleVariety);
        }

        void Start()
        {
            seedInput.text = seed;
            maxRestartsInput.text = maxRestarts.ToString();
        }

        private void InitRandom()
        {
            int usedSeed;
            if (!string.IsNullOrEmpty(seed))
            {
                usedSeed = seed.GetHashCode();
            }
            else
            {
                usedSeed = Environment.TickCount;
            }

            Random.InitState(usedSeed);
            Debug.Log("[WFC3D] using seed: " + usedSeed);
        }

        private bool TryRun(out int[,,] result, int attempts)
        {
            result = new int[width, depth, height];
            int moduleCount = modules.Length;

            List<int>[,] compatibility = new List<int>[moduleCount, 6];
            for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
            {
                for (int direction = 0; direction < 6; direction++)
                {
                    compatibility[moduleIndex, direction] = new List<int>();
                }
            }

            for (int moduleAIndex = 0; moduleAIndex < moduleCount; moduleAIndex++)
            {
                for (int moduleBIndex = 0; moduleBIndex < moduleCount; moduleBIndex++)
                {
                    if (modules[moduleAIndex] == null || modules[moduleBIndex] == null)
                    {
                        continue;
                    }

                    if (modules[moduleAIndex].GetPort(0) == modules[moduleBIndex].GetPort(1))
                        compatibility[moduleAIndex, 0].Add(moduleBIndex);

                    if (modules[moduleAIndex].GetPort(1) == modules[moduleBIndex].GetPort(0))
                        compatibility[moduleAIndex, 1].Add(moduleBIndex);

                    if (modules[moduleAIndex].GetPort(2) == modules[moduleBIndex].GetPort(3))
                        compatibility[moduleAIndex, 2].Add(moduleBIndex);

                    if (modules[moduleAIndex].GetPort(3) == modules[moduleBIndex].GetPort(2))
                        compatibility[moduleAIndex, 3].Add(moduleBIndex);

                    if (modules[moduleAIndex].GetPort(4) == modules[moduleBIndex].GetPort(5))
                        compatibility[moduleAIndex, 4].Add(moduleBIndex);

                    if (modules[moduleAIndex].GetPort(5) == modules[moduleBIndex].GetPort(4))
                        compatibility[moduleAIndex, 5].Add(moduleBIndex);
                }
            }

            HashSet<int>[,,] possible = new HashSet<int>[width, depth, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        possible[x, y, z] = new HashSet<int>();
                        for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
                        {
                            possible[x, y, z].Add(moduleIndex);
                        }
                    }
                }
            }

            (int dx, int dy, int dz, int opposite)[] neighbours =
            {
                (1, 0, 0, 1),
                (-1, 0, 0, 0),
                (0, 0, 1, 3),
                (0, 0, -1, 2),
                (0, 1, 0, 5),
                (0, -1, 0, 4)
            };

            Queue<(int x, int y, int z)> propagationQueue = new Queue<(int, int, int)>();

            int totalPropagationSteps = 0;

            int collapsedCount = 0;
            int totalCells = width * depth * height;

            while (collapsedCount < totalCells)
            {
                int bestX = -1, bestY = -1, bestZ = -1;
                int bestCount = int.MaxValue;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < depth; y++)
                    {
                        for (int z = 0; z < height; z++)
                        {
                            int candidateCount = possible[x, y, z].Count;
                            if (candidateCount == 1)
                            {
                                continue;
                            }

                            if (candidateCount < bestCount)
                            {
                                bestCount = candidateCount;
                                bestX = x;
                                bestY = y;
                                bestZ = z;
                            }
                        }
                    }
                }

                if (bestX == -1)
                {
                    break;
                }

                List<int> choices = new List<int>(possible[bestX, bestY, bestZ]);
                if (choices.Count == 0)
                {
                    return false;
                }

                int chosenModule = choices[Random.Range(0, choices.Count)];

                possible[bestX, bestY, bestZ].Clear();
                possible[bestX, bestY, bestZ].Add(chosenModule);

                propagationQueue.Enqueue((bestX, bestY, bestZ));

                bool contradiction = false;
                while (propagationQueue.Count > 0 && !contradiction)
                {
                    var (currX, currY, currZ) = propagationQueue.Dequeue();

                    totalPropagationSteps++;

                    for (int dir = 0; dir < neighbours.Length; dir++)
                    {
                        int nx = currX + neighbours[dir].dx;
                        int ny = currY + neighbours[dir].dy;
                        int nz = currZ + neighbours[dir].dz;

                        if (nx < 0 || nx >= width || ny < 0 || ny >= depth || nz < 0 || nz >= height)
                        {
                            continue;
                        }

                        HashSet<int> allowedForNeighbour = new HashSet<int>();
                        foreach (int moduleHere in possible[currX, currY, currZ])
                        {
                            List<int> compatibleList = compatibility[moduleHere, dir];
                            for (int compatIdx = 0; compatIdx < compatibleList.Count; compatIdx++)
                            {
                                allowedForNeighbour.Add(compatibleList[compatIdx]);
                            }
                        }

                        int beforeCount = possible[nx, ny, nz].Count;
                        List<int> toRemove = new List<int>();
                        foreach (int candidate in possible[nx, ny, nz])
                        {
                            if (!allowedForNeighbour.Contains(candidate))
                            {
                                toRemove.Add(candidate);
                            }
                        }

                        for (int r = 0; r < toRemove.Count; r++)
                        {
                            possible[nx, ny, nz].Remove(toRemove[r]);
                        }

                        int afterCount = possible[nx, ny, nz].Count;
                        if (afterCount == 0)
                        {
                            contradiction = true;
                            break;
                        }

                        if (afterCount < beforeCount)
                        {
                            propagationQueue.Enqueue((nx, ny, nz));
                        }
                    }
                }

                if (contradiction)
                {
                    return false;
                }

                collapsedCount = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < depth; y++)
                    {
                        for (int z = 0; z < height; z++)
                            if (possible[x, y, z].Count == 1)
                            {
                                collapsedCount++;
                            }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                        result[x, y, z] = FirstOf(possible[x, y, z]);
                }
            }

            LogWFCMetrics(attempts, totalPropagationSteps, possible);
            return true;
        }

        private int FirstOf(HashSet<int> s)
        {
            foreach (int v in s)
            {
                return v;
            }

            return 0;
        }

        private void InstantiateResult(int[,,] result)
        {
            Transform old = transform.Find("WFC3D_Generated");
            if (old != null)
            {
                if (Application.isPlaying)
                    Destroy(old.gameObject);
                else
                    DestroyImmediate(old.gameObject);
            }

            GameObject root = new GameObject("WFC3D_Generated");
            root.transform.SetParent(transform, false);

            Transform targetParent = parentForPrefabs != null ? parentForPrefabs : root.transform;

            if (parentForPrefabs != null)
            {
                for (int i = parentForPrefabs.childCount - 1; i >= 0; i--)
                {
                    Transform ch = parentForPrefabs.GetChild(i);
                    if (Application.isPlaying)
                        Destroy(ch.gameObject);
                    else
                        DestroyImmediate(ch.gameObject);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        int idx = result[x, y, z];
                        if (idx < 0 || idx >= modules.Length || modules[idx] == null || modules[idx].prefab == null)
                        {
                            continue;
                        }

                        Vector3 pos = GridToWorld(x, y, z);
                        Quaternion rot = Quaternion.identity;
                        GameObject go = Instantiate(modules[idx].prefab, pos, rot, targetParent);
                        go.transform.localScale = Vector3.one * cellSize;
                    }
                }
            }
        }

        private Vector3 GridToWorld(int gx, int gy, int gz)
        {
            float ox = -width * cellSize * 0.5f + cellSize * 0.5f;
            float oy = -depth * cellSize * 0.5f + cellSize * 0.5f;
            float oz = -height * cellSize * 0.5f + cellSize * 0.5f;
            Vector3 local = new Vector3(ox + gx * cellSize, oy + gy * cellSize, oz + gz * cellSize);
            return transform.TransformPoint(local);
        }

        private void ClearPrefabs()
        {
            if (parentForPrefabs != null)
            {
                for (int i = parentForPrefabs.childCount - 1; i >= 0; i--)
                {
                    Transform child = parentForPrefabs.GetChild(i);
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }

            Transform oldRoot = transform.Find("WFC3D_Generated");
            if (oldRoot != null)
            {
                if (Application.isPlaying)
                    Destroy(oldRoot.gameObject);
                else
                    DestroyImmediate(oldRoot.gameObject);
            }
        }
    }
}