using UnityEngine;
using Unity.Profiling;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

public class Metrics : MonoBehaviour 
{
    public static Metrics Instance;
    
    [Header("UI")]
    public TMP_Text statsText;
    public TMP_Text infoText;
    private float _uiUpdateTimer;
    private const float UIUpdateDelay = 0.5f;
    private float _lastFps;
    private double _lastCpuMs, _lastGpuMs, _lastMemMb;
    
    [Header("Export")]
    public string fileName = "PCG_Metrics";

    ProfilerRecorder _mainThreadTime;
    ProfilerRecorder _gpuFrameTime;
    ProfilerRecorder _gcAlloc;
    ProfilerRecorder _totalMem;

    [System.Serializable]
    public struct PcgStats
    {
        public string name;
        public int width, height;
        public double genTimeMs;
        public float avgFps, minFps;
        public double cpuMs, gpuMs, gcMb, totalMemMb;
        
        public float avgValue, stdDev, contrast;
        public int scale;
        
        public int stringLength, segmentCount, branchFactor, maxDepth;
        
        public int deadEnds, regions, iterations;
        public float fillPct, birthThreshold;
        
        public int attempts, propagationSteps;
        public float avgEntropy, moduleVariety;
        
        public string timestamp;
        public string unityVersion;
        public string platform;
    }

    public List<PcgStats> pcgResults;
    private PcgStats _currentPcg;
    private double _pcgStartTime;
    
    private bool _isRecordingFps;
    public bool CanGenerate => !_isRecordingFps;
    private float _fpsSum;
    private float _fpsMin;
    private int _fpsCount;
    private float _fpsTimer;
    private const float FpsMeasureDuration = 3f;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); 
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        _mainThreadTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
        _gpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GPU Frame Time");
        _gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        _totalMem = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
    }

    void OnDisable()
    {
        _mainThreadTime.Dispose();
        _gpuFrameTime.Dispose();
        _gcAlloc.Dispose();
        _totalMem.Dispose();
    }
    
    void Update() 
    {
        float fps = Time.unscaledDeltaTime > 0 ? 1f / Time.unscaledDeltaTime : 0f;
        _lastFps = fps;

        if (_isRecordingFps)
        {
            _fpsTimer += Time.unscaledDeltaTime;
            _fpsSum += fps;
            _fpsCount++;
            
            if (fps < _fpsMin) 
                _fpsMin = fps;

            if (_fpsTimer >= FpsMeasureDuration)
            {
                _isRecordingFps = false;
                _currentPcg.avgFps = _fpsCount > 0 ? _fpsSum / _fpsCount : 0f;
                _currentPcg.minFps = Mathf.Approximately(_fpsMin, float.MaxValue) ? 0f : _fpsMin;
                _currentPcg.cpuMs = _mainThreadTime.LastValue * 1e-6;
                _currentPcg.gpuMs = _gpuFrameTime.LastValue * 1e-6;
                _currentPcg.gcMb = _gcAlloc.LastValue / (1024.0 * 1024.0);
                _currentPcg.totalMemMb = _totalMem.LastValue / (1024.0 * 1024.0);
                pcgResults.Add(_currentPcg);
            }
        }
        
        _uiUpdateTimer += Time.unscaledDeltaTime;
        if (_uiUpdateTimer >= UIUpdateDelay)
        {
            _uiUpdateTimer = 0f;
            _lastCpuMs = _mainThreadTime.LastValue * 1e-6;
            _lastGpuMs = _gpuFrameTime.LastValue * 1e-6;
            _lastMemMb = _totalMem.LastValue / (1024.0 * 1024.0);
        
            if (statsText == null) return;
            statsText.text = $"FPS: {_lastFps:F0}\n" +
                             $"Gen: {_currentPcg.genTimeMs:F1}ms\n" +
                             $"CPU: {_lastCpuMs:F1}ms\n" +
                             $"GPU: {_lastGpuMs:F1}ms\n" +
                             $"Mem: {_lastMemMb:F1}MB";
        }
        
        if (infoText != null)
        {
            if (_isRecordingFps)
            {
                float progress = _fpsTimer / FpsMeasureDuration * 100f;
                infoText.text = $"Meranie výkonu: {progress:F0}%";
            }
            else
            {
                infoText.text = $"Pripravený | Záznamy: {pcgResults.Count}";
            }
        }
    }
    
    public void ExportCsv()
    {
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine(
            "timestamp,unityVersion,platform," +
            "name,width,height," +
            "genTimeMs,avgFps,minFps,cpuMs,gpuMs,gcMb,totalMemMb," +
            "deadEnds,regions,fillPct,iterations,birthThreshold," +
            "stringLength,segmentCount,branchFactor,maxDepth," +
            "attempts,propagationSteps,avgEntropy,moduleVariety," +
            "avgValue,stdDev,contrast,scale");
    
        foreach (var r in pcgResults)
        {
            sb.AppendLine(
                $"{r.timestamp},{r.unityVersion},{r.platform}," +
                $"{r.name},{r.width},{r.height}," +
                $"{r.genTimeMs.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.avgFps.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.minFps.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.cpuMs.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.gpuMs.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.gcMb.ToString("F3", CultureInfo.InvariantCulture)}," +
                $"{r.totalMemMb.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.deadEnds},{r.regions}," +
                $"{r.fillPct.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.iterations}," +
                $"{r.birthThreshold.ToString("F1", CultureInfo.InvariantCulture)}," +
                $"{r.stringLength},{r.segmentCount},{r.branchFactor},{r.maxDepth}," +
                $"{r.attempts},{r.propagationSteps}," +
                $"{r.avgEntropy.ToString("F3", CultureInfo.InvariantCulture)}," +
                $"{r.moduleVariety.ToString("F3", CultureInfo.InvariantCulture)}," +
                $"{r.avgValue.ToString("F3", CultureInfo.InvariantCulture)}," +
                $"{r.stdDev.ToString("F3", CultureInfo.InvariantCulture)}," +
                $"{r.contrast.ToString("F3", CultureInfo.InvariantCulture)}," +
                $"{r.scale}");
        }
    
        string buildFolder = Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath;
        string filename = $"{fileName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string path = Path.Combine(buildFolder, filename);
    
        File.WriteAllText(path, sb.ToString());
    }
    
    public void UpdateUI(TMP_Text stats, TMP_Text info)
    {
        statsText = stats;
        infoText = info;
    }

    public void StartPcg(string pcgName, int width = 64, int height = 64)
    {
        _pcgStartTime = Time.realtimeSinceStartupAsDouble;
        _currentPcg = new PcgStats 
        { 
            name = pcgName,
            width = width,
            height = height,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            unityVersion = Application.unityVersion,
            platform = Application.platform.ToString()
        };
    }
    
    public void EndPcg()
    {
        _currentPcg.genTimeMs = (Time.realtimeSinceStartupAsDouble - _pcgStartTime) * 1000;
        
        _fpsSum = 0;
        _fpsMin = float.MaxValue;
        _fpsCount = 0;
        _fpsTimer = 0;
        _isRecordingFps = true;
    }
    
    public void LogCa(int deadEnds, int regions, float fillPct, int iterations, float birthThreshold = 0)
    {
        _currentPcg.deadEnds = deadEnds;
        _currentPcg.regions = regions;
        _currentPcg.fillPct = fillPct;
        _currentPcg.iterations = iterations;
        _currentPcg.birthThreshold = birthThreshold;
    }
    
    public void LogPerlin(float avgValue, float stdDev, float contrast, int scale = 0)
    {
        _currentPcg.avgValue = avgValue;
        _currentPcg.stdDev = stdDev;
        _currentPcg.contrast = contrast;
        _currentPcg.scale = scale;
    }
    
    public void LogLSystem(int stringLength, int segmentCount, int branchFactor, int maxDepth)
    {
        _currentPcg.stringLength = stringLength;
        _currentPcg.segmentCount = segmentCount;
        _currentPcg.branchFactor = branchFactor;
        _currentPcg.maxDepth = maxDepth;
    }
    
    public void LogWfc(int attempts, int propagationSteps, float avgEntropy, float moduleVariety)
    {
        _currentPcg.attempts = attempts;
        _currentPcg.propagationSteps = propagationSteps;
        _currentPcg.avgEntropy = avgEntropy;
        _currentPcg.moduleVariety = moduleVariety;
    }
    
    public void LogDeadEnds(int count)
    {
        _currentPcg.deadEnds = count;
    }
    
    public void LogRegions(int count)
    {
        _currentPcg.regions = count;
    }
    
    public void LogFillPct(float pct)
    {
        _currentPcg.fillPct = pct;
    }
}