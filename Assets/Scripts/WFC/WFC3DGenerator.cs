// WFC kód inšpirácia od Maxim Gumin
// https://github.com/mxgmn/WaveFunctionCollapse

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WFC
{
    public class Wfc3DGenerator : MonoBehaviour
    {
        [Header("Grid")]
        public int width = 8;
        public int height = 8;
        public int depth = 4;
        public float cellSize = 1f;

        [Header("Modules")]
        public ModuleDef[] modules;

        [Header("Random")]
        public string seed;
        public int maxRestarts = 20;

        public TMP_InputField widthInput;
        public TMP_InputField heightInput;
        public TMP_InputField seedInput;
        public TMP_InputField maxRestartsInput;

        [Header("Output")]
        public Transform parentForPrefabs;

        private int[,,] _generatedGrid;
        private bool _generationSuccessful;
        private int _propagationStepCount;
        private HashSet<int>[,,] _possibleStates;

        private int[] _allowedModuleMarks;
        private int _allowedModuleToken = 1;
        private readonly List<int> _modulesToRemove = new List<int>(128);

        private struct CellPriorityEntry : IComparable<CellPriorityEntry>
        {
            public int x;
            public int y;
            public int z;
            public int count;
            public int order;

            public int CompareTo(CellPriorityEntry other)
            {
                int compare = count.CompareTo(other.count);
                if (compare != 0) return compare;

                compare = order.CompareTo(other.order);
                if (compare != 0) return compare;

                compare = x.CompareTo(other.x);
                if (compare != 0) return compare;

                compare = y.CompareTo(other.y);
                if (compare != 0) return compare;

                return z.CompareTo(other.z);
            }
        }

        [ContextMenu("Generate")]
        public void GenerateWFC()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;

            if (widthInput != null && !string.IsNullOrEmpty(widthInput.text))
                width = int.Parse(widthInput.text);

            if (heightInput != null && !string.IsNullOrEmpty(heightInput.text))
                height = int.Parse(heightInput.text);

            seed = seedInput?.text ?? "";

            if (maxRestartsInput != null && !string.IsNullOrEmpty(maxRestartsInput.text))
                maxRestarts = int.Parse(maxRestartsInput.text);

            ClearPrefabs();
            Generate();
        }

        [ContextMenu("Clear Parent")]
        public void ClearParentContext()
        {
            ClearPrefabs();
        }

        public void ClearParent()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;

            ClearPrefabs();
        }

        private void Generate()
        {
            Metrics.Instance?.StartPcg("WFC3DGeneration", width, height);

            if (modules == null || modules.Length == 0)
            {
                Debug.LogError("[WFC3D] modules not assigned.");
                Metrics.Instance?.EndPcg();
                return;
            }

            int attemptNumber = 0;
            _generationSuccessful = false;

            while (attemptNumber < maxRestarts && !_generationSuccessful)
            {
                attemptNumber++;
                InitRandom();

                _generationSuccessful = TryRun(out _generatedGrid, out _propagationStepCount, out _possibleStates);

                if (!_generationSuccessful)
                {
                    Debug.LogWarning("[WFC3D] attempt " + attemptNumber + " failed, retrying...");
                }
                else
                {
                    Debug.Log("[WFC3D] success on attempt " + attemptNumber);
                    LogWfcMetrics(attemptNumber, _propagationStepCount, _possibleStates);
                    InstantiateResult(_generatedGrid);
                }
            }

            if (!_generationSuccessful)
                Debug.LogError("[WFC3D] generation failed after maxRestarts=" + maxRestarts);

            Metrics.Instance?.EndPcg();
        }

        private void LogWfcMetrics(int attemptNumber, int propagationSteps, HashSet<int>[,,] states)
        {
            float averageEntropy = 0f;
            int totalCells = width * depth * height;
            int cellCount = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        averageEntropy += states[x, y, z].Count;
                        cellCount++;
                    }
                }
            }

            averageEntropy = cellCount > 0 ? averageEntropy / cellCount : 0f;
            float moduleVariety = modules.Length > 0 ? cellCount / (float)(modules.Length * totalCells) : 0f;

            Metrics.Instance?.LogWfc(attemptNumber, propagationSteps, averageEntropy, moduleVariety);
        }

        void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            if (widthInput != null)
                widthInput.text = width.ToString();

            if (heightInput != null)
                heightInput.text = height.ToString();

            if (seedInput != null)
                seedInput.text = seed;

            if (maxRestartsInput != null)
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

        private bool TryRun(out int[,,] result, out int propagationSteps, out HashSet<int>[,,] states)
        {
            int moduleCount = modules.Length;
            int totalCells = width * depth * height;

            result = new int[width, depth, height];
            propagationSteps = 0;
            states = new HashSet<int>[width, depth, height];

            if (_allowedModuleMarks == null || _allowedModuleMarks.Length != moduleCount)
                _allowedModuleMarks = new int[moduleCount];

            if (_allowedModuleToken == int.MaxValue - 1000)
            {
                Array.Clear(_allowedModuleMarks, 0, _allowedModuleMarks.Length);
                _allowedModuleToken = 1;
            }

            _modulesToRemove.Clear();

            List<int>[][] compatibilityByModuleAndDirection = BuildCompatibilityTable(moduleCount);

            HashSet<int>[,,] possibleModuleStates = new HashSet<int>[width, depth, height];
            SortedSet<CellPriorityEntry> cellPriorityQueue = new SortedSet<CellPriorityEntry>();
            int queueOrder = 0;
            int collapsedCellCount = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        possibleModuleStates[x, y, z] = new HashSet<int>(moduleCount);

                        for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
                        {
                            possibleModuleStates[x, y, z].Add(moduleIndex);
                        }

                        cellPriorityQueue.Add(new CellPriorityEntry
                        {
                            x = x,
                            y = y,
                            z = z,
                            count = moduleCount,
                            order = queueOrder++
                        });
                    }
                }
            }

            states = possibleModuleStates;

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

            while (collapsedCellCount < totalCells)
            {
                if (!TryPopNextCell(cellPriorityQueue, possibleModuleStates, out CellPriorityEntry selectedCell))
                    break;

                HashSet<int> selectedCellStates = possibleModuleStates[selectedCell.x, selectedCell.y, selectedCell.z];
                if (selectedCellStates.Count <= 1)
                    continue;

                int chosenModule = PickRandomFromSet(selectedCellStates);

                selectedCellStates.Clear();
                selectedCellStates.Add(chosenModule);
                collapsedCellCount++;

                propagationQueue.Enqueue((selectedCell.x, selectedCell.y, selectedCell.z));

                bool contradictionFound = false;

                while (propagationQueue.Count > 0 && !contradictionFound)
                {
                    var (currentX, currentY, currentZ) = propagationQueue.Dequeue();
                    propagationSteps++;

                    for (int directionIndex = 0; directionIndex < neighbours.Length; directionIndex++)
                    {
                        int nextX = currentX + neighbours[directionIndex].dx;
                        int nextY = currentY + neighbours[directionIndex].dy;
                        int nextZ = currentZ + neighbours[directionIndex].dz;

                        if (nextX < 0 || nextX >= width ||
                            nextY < 0 || nextY >= depth ||
                            nextZ < 0 || nextZ >= height)
                        {
                            continue;
                        }

                        HashSet<int> neighbourStates = possibleModuleStates[nextX, nextY, nextZ];
                        if (neighbourStates.Count == 0)
                        {
                            contradictionFound = true;
                            break;
                        }

                        MarkAllowedModules(possibleModuleStates[currentX, currentY, currentZ], compatibilityByModuleAndDirection, directionIndex);

                        _modulesToRemove.Clear();
                        foreach (int candidate in neighbourStates)
                        {
                            if (_allowedModuleMarks[candidate] != _allowedModuleToken)
                                _modulesToRemove.Add(candidate);
                        }

                        int beforeCount = neighbourStates.Count;

                        for (int removeIndex = 0; removeIndex < _modulesToRemove.Count; removeIndex++)
                        {
                            neighbourStates.Remove(_modulesToRemove[removeIndex]);
                        }

                        int afterCount = neighbourStates.Count;

                        if (afterCount == 0)
                        {
                            contradictionFound = true;
                            break;
                        }

                        if (afterCount < beforeCount)
                        {
                            if (beforeCount > 1 && afterCount == 1)
                                collapsedCellCount++;

                            cellPriorityQueue.Add(new CellPriorityEntry
                            {
                                x = nextX,
                                y = nextY,
                                z = nextZ,
                                count = afterCount,
                                order = queueOrder++
                            });

                            propagationQueue.Enqueue((nextX, nextY, nextZ));
                        }
                    }
                }

                if (contradictionFound)
                {
                    _generatedGrid = null;
                    _generationSuccessful = false;
                    _propagationStepCount = propagationSteps;
                    states = possibleModuleStates;
                    return false;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        result[x, y, z] = possibleModuleStates[x, y, z].Count > 0
                            ? FirstOf(possibleModuleStates[x, y, z])
                            : 0;
                    }
                }
            }

            _generatedGrid = result;
            _generationSuccessful = true;
            _propagationStepCount = propagationSteps;
            states = possibleModuleStates;
            return true;
        }

        private List<int>[][] BuildCompatibilityTable(int moduleCount)
        {
            List<int>[][] compatibilityByModuleAndDirection = new List<int>[moduleCount][];

            for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
            {
                compatibilityByModuleAndDirection[moduleIndex] = new List<int>[6];
                for (int directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    compatibilityByModuleAndDirection[moduleIndex][directionIndex] = new List<int>();
                }
            }

            for (int moduleIndexA = 0; moduleIndexA < moduleCount; moduleIndexA++)
            {
                for (int moduleIndexB = 0; moduleIndexB < moduleCount; moduleIndexB++)
                {
                    if (modules[moduleIndexA] == null || modules[moduleIndexB] == null)
                        continue;

                    if (modules[moduleIndexA].GetPort(0) == modules[moduleIndexB].GetPort(1))
                        compatibilityByModuleAndDirection[moduleIndexA][0].Add(moduleIndexB);

                    if (modules[moduleIndexA].GetPort(1) == modules[moduleIndexB].GetPort(0))
                        compatibilityByModuleAndDirection[moduleIndexA][1].Add(moduleIndexB);

                    if (modules[moduleIndexA].GetPort(2) == modules[moduleIndexB].GetPort(3))
                        compatibilityByModuleAndDirection[moduleIndexA][2].Add(moduleIndexB);

                    if (modules[moduleIndexA].GetPort(3) == modules[moduleIndexB].GetPort(2))
                        compatibilityByModuleAndDirection[moduleIndexA][3].Add(moduleIndexB);

                    if (modules[moduleIndexA].GetPort(4) == modules[moduleIndexB].GetPort(5))
                        compatibilityByModuleAndDirection[moduleIndexA][4].Add(moduleIndexB);

                    if (modules[moduleIndexA].GetPort(5) == modules[moduleIndexB].GetPort(4))
                        compatibilityByModuleAndDirection[moduleIndexA][5].Add(moduleIndexB);
                }
            }

            return compatibilityByModuleAndDirection;
        }

        private void MarkAllowedModules(
            HashSet<int> sourceCellStates,
            List<int>[][] compatibilityByModuleAndDirection,
            int directionIndex)
        {
            _allowedModuleToken++;
            if (_allowedModuleToken == int.MaxValue)
            {
                Array.Clear(_allowedModuleMarks, 0, _allowedModuleMarks.Length);
                _allowedModuleToken = 1;
            }

            foreach (int sourceModuleIndex in sourceCellStates)
            {
                foreach (int compatibleModuleIndex in compatibilityByModuleAndDirection[sourceModuleIndex][directionIndex])
                {
                    _allowedModuleMarks[compatibleModuleIndex] = _allowedModuleToken;
                }
            }
        }

        private bool TryPopNextCell(
            SortedSet<CellPriorityEntry> cellPriorityQueue,
            HashSet<int>[,,] possibleModuleStates,
            out CellPriorityEntry selectedCell)
        {
            while (cellPriorityQueue.Count > 0)
            {
                selectedCell = cellPriorityQueue.Min;
                cellPriorityQueue.Remove(selectedCell);

                int currentCount = possibleModuleStates[selectedCell.x, selectedCell.y, selectedCell.z].Count;
                if (currentCount > 1 && currentCount == selectedCell.count)
                    return true;
            }

            selectedCell = default;
            return false;
        }

        private int PickRandomFromSet(HashSet<int> stateSet)
        {
            int targetIndex = Random.Range(0, stateSet.Count);
            int currentIndex = 0;

            foreach (int value in stateSet)
            {
                if (currentIndex == targetIndex)
                    return value;

                currentIndex++;
            }

            return 0;
        }

        private int FirstOf(HashSet<int> stateSet)
        {
            foreach (int value in stateSet)
            {
                return value;
            }

            return 0;
        }

        private void InstantiateResult(int[,,] resultGrid)
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
                for (int childIndex = parentForPrefabs.childCount - 1; childIndex >= 0; childIndex--)
                {
                    Transform child = parentForPrefabs.GetChild(childIndex);
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        int moduleIndex = resultGrid[x, y, z];
                        if (moduleIndex < 0 || moduleIndex >= modules.Length || modules[moduleIndex] == null || modules[moduleIndex].prefab == null)
                            continue;

                        Vector3 position = GridToWorld(x, y, z);
                        Quaternion rotation = Quaternion.identity;
                        GameObject spawnedObject = Instantiate(modules[moduleIndex].prefab, position, rotation, targetParent);
                        spawnedObject.transform.localScale = Vector3.one * cellSize;
                    }
                }
            }
        }

        private Vector3 GridToWorld(int gx, int gy, int gz)
        {
            float offsetX = -width * cellSize * 0.5f + cellSize * 0.5f;
            float offsetY = -depth * cellSize * 0.5f + cellSize * 0.5f;
            float offsetZ = -height * cellSize * 0.5f + cellSize * 0.5f;

            Vector3 localPosition = new Vector3(
                offsetX + gx * cellSize,
                offsetY + gy * cellSize,
                offsetZ + gz * cellSize
            );

            return transform.TransformPoint(localPosition);
        }

        private void ClearPrefabs()
        {
            if (parentForPrefabs != null)
            {
                for (int childIndex = parentForPrefabs.childCount - 1; childIndex >= 0; childIndex--)
                {
                    Transform child = parentForPrefabs.GetChild(childIndex);
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