using UnityEngine;
using Unity.Profiling;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

public class Telemetry : MonoBehaviour 
{
    public static Telemetry Instance;
    
    [Header("UI")]
    public TMP_Text statsText;
    
    [Header("Export")]
    public string fileName = "PCG_Telemetry";
    
    ProfilerRecorder _cpuFrame, _mainThread, _gpuFrame;
    ProfilerRecorder _gcAlloc, _totalMem, _drawCalls;
    
    struct FrameSample
    {
        public int frame;
        public double cpuFrame, mainThread, gpuFrame;
        public double gcAlloc, totalMem;
        public long drawCalls;
    }
    
    [System.Serializable]
    public struct PCGStats
    {
        public string name, size;
        public int width, height;
        public double genTimeMs, cpuMs, gpuMs, gcMB;
        
        public float avgValue, stdDev, contrast;
        public int octaves;
        
        public int stringLength, segmentCount, branchFactor, maxDepth;
        public float totalLength;
        
        public int deadEnds, regions, iterations;
        public float fillPct, birthThreshold;
        public bool stable;
        
        public int attempts, propagationSteps;
        public float avgEntropy, moduleVariety;
    }

    
    public List<PCGStats> pcgResults = new List<PCGStats>();
    private PCGStats _currentPCG;
    private double _pcgStartTime;
    
    List<FrameSample> _samples = new List<FrameSample>();
    float _timer;
    const float BenchmarkDuration = 60f;
    bool _exported;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); 
            return;
        }
        Instance = this;
    }
    
    void OnEnable() 
    {
        _cpuFrame = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Frame Time");
        _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        _gpuFrame = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        _gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        _totalMem = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
        _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
    }
    
    void OnDisable() 
    {
        _cpuFrame.Dispose();
        _mainThread.Dispose();
        _gpuFrame.Dispose();
        _gcAlloc.Dispose();
        _totalMem.Dispose();
        _drawCalls.Dispose();
    }
    
    void Update() 
    {
        _timer += Time.unscaledDeltaTime;
        
        if (_timer < BenchmarkDuration)
        {
            FrameSample s = new FrameSample
            {
                frame = Time.frameCount,
                cpuFrame = _cpuFrame.LastValue * 1e-6,
                mainThread = _mainThread.LastValue * 1e-6,
                gpuFrame = _gpuFrame.LastValue * 1e-6,
                gcAlloc = _gcAlloc.LastValue / (1024.0 * 1024.0),
                totalMem = _totalMem.LastValue / (1024.0 * 1024.0),
                drawCalls = _drawCalls.LastValue
            };
            _samples.Add(s);
        }
        
        if (_timer > BenchmarkDuration && !_exported)
        {
            ExportCsv();
            _exported = true;
        }
        
        if (!statsText) return;
        statsText.text = $"CPU: {_mainThread.LastValue * 1e-6:F1}ms\n" +
                         $"GPU: {_gpuFrame.LastValue * 1e-6:F1}ms\n" +
                         $"GC: {_gcAlloc.LastValue / (1024*1024):F1}MB\n" +
                         $"Draw: {_drawCalls.LastValue}\n" +
                         $"Mem: {_totalMem.LastValue / (1024*1024):F1}MB";
    }
    
    void ExportCsv()
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("PROFILER");
        sb.AppendLine("frame,cpuFrame,mainThread,gpuFrame,gcAlloc,totalMem,drawCalls");
    
        foreach (var s in _samples)
        {
            sb.AppendLine($"{s.frame}," +
                          $"{s.cpuFrame.ToString(CultureInfo.InvariantCulture)}," +
                          $"{s.mainThread.ToString(CultureInfo.InvariantCulture)}," +
                          $"{s.gpuFrame.ToString(CultureInfo.InvariantCulture)}," +
                          $"{s.gcAlloc.ToString(CultureInfo.InvariantCulture)}," +
                          $"{s.totalMem.ToString(CultureInfo.InvariantCulture)}," +
                          $"{s.drawCalls}");
        }
        
        sb.AppendLine("\nPCG");
        sb.AppendLine("name,size,width,height,genTimeMs,cpuMs,gpuMs,gcMB,deadEnds,regions,fillPct," +
                      "iterations,birthThreshold,stringLength,segmentCount,attempts,avgValue,stdDev");
    
        foreach (var result in pcgResults)
        {
            sb.AppendLine($"{result.name},{result.size},{result.width},{result.height}," +
                          $"{result.genTimeMs:F1},{result.cpuMs:F1},{result.gpuMs:F1},{result.gcMB:F1}," +
                          $"{result.deadEnds},{result.regions},{result.fillPct:F1}," +
                          $"{result.iterations},{result.birthThreshold:F1},{result.stringLength}," +
                          $"{result.segmentCount},{result.attempts},{result.avgValue:F1},{result.stdDev:F1}");
        }
    
        string buildFolder = Path.GetDirectoryName(Application.dataPath);
        string filename = $"{fileName}_COMPLETE_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(buildFolder, filename);
    
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"COMPLETE EXPORT: {path} | Frames: {_samples.Count} | PCG: {pcgResults.Count}");
    }

    
    public void StartPCG(string name, string size = "Medium", int width = 64, int height = 64)
    {
        _pcgStartTime = Time.realtimeSinceStartupAsDouble;
        _currentPCG = new PCGStats 
        { 
            name = name, size = size, 
            width = width, height = height,
            deadEnds = 0, regions = 0, fillPct = 0f
        };
        
        using var _ = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, $"PCG_{name}");
    }
    
    public void EndPCG()
    {
        _currentPCG.genTimeMs = (Time.realtimeSinceStartupAsDouble - _pcgStartTime) * 1000;

        double cpuSum = 0;
        double gpuSum = 0;
        double gcSum = 0;
        int count = 0;

        foreach (var s in _samples)
        {
            cpuSum += s.mainThread;
            gpuSum += s.gpuFrame;
            gcSum += s.gcAlloc;
            count++;
        }

        if (count > 0)
        {
            _currentPCG.cpuMs = cpuSum / count;
            _currentPCG.gpuMs = gpuSum / count;
            _currentPCG.gcMB = gcSum / count;
        }

        pcgResults.Add(_currentPCG);
        Debug.Log($"PCG [{_currentPCG.name}] {_currentPCG.size}: {_currentPCG.genTimeMs:F1}ms");
    }
    
    public void LogCA(int deadEnds, int regions, float fillPct, int iterations, float birthThreshold = 0)
    {
        _currentPCG.deadEnds = deadEnds;
        _currentPCG.regions = regions;
        _currentPCG.fillPct = fillPct;
        _currentPCG.iterations = iterations;
        _currentPCG.birthThreshold = birthThreshold;
    }
    
    public void LogPerlin(float avgValue, float stdDev, float contrast, int octaves = 0)
    {
        _currentPCG.avgValue = avgValue;
        _currentPCG.stdDev = stdDev;
        _currentPCG.contrast = contrast;
        _currentPCG.octaves = octaves;
    }
    
    public void LogLSystem(int stringLength, int segmentCount, int branchFactor, int maxDepth)
    {
        _currentPCG.stringLength = stringLength;
        _currentPCG.segmentCount = segmentCount;
        _currentPCG.branchFactor = branchFactor;
        _currentPCG.maxDepth = maxDepth;
    }
    
    public void LogWFC(int attempts, int propagationSteps, float avgEntropy, float moduleVariety)
    {
        _currentPCG.attempts = attempts;
        _currentPCG.propagationSteps = propagationSteps;
        _currentPCG.avgEntropy = avgEntropy;
        _currentPCG.moduleVariety = moduleVariety;
    }
    
    public void LogDeadEnds(int count)
    {
        _currentPCG.deadEnds = count;
    }
    
    public void LogRegions(int count)
    {
        _currentPCG.regions = count;
    }
    
    public void LogFillPct(float pct)
    {
        _currentPCG.fillPct = pct;
    }
}