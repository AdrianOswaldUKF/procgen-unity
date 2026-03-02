using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Telemetry : MonoBehaviour
{
    public static Telemetry Instance;
    
    public float fpsUpdateInterval = 0.5f;
    
    public float CurrentFps;
    public bool IsGenerating;
    public float CurrentElapsedMs;
    
    public TMP_Text fpsText;
    private TMP_Text genLabelText;
    public TMP_Text genTimeText;
    
    private string CurrentLabel;
    private double _genStartTime = -1.0;
    private float _fpsAccumTime;
    private int _fpsFrames;
    private Dictionary<string, float> _lastGenMs;
    private string _lastGenLabel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        if (_lastGenMs == null)
        {
            _lastGenMs = new Dictionary<string, float>();
        }
    }
    
    public void RecordGenerationStart(string label)
    {
        _genStartTime = Time.realtimeSinceStartupAsDouble;
        IsGenerating = true;
        CurrentLabel = string.IsNullOrEmpty(label) ? "gen" : label;
        CurrentElapsedMs = 0f;
    }
    
    public void RecordGenerationEnd(string label)
    {
        if (_genStartTime < 0.0)
        {
            return;
        }

        if (_lastGenMs == null)
        {
            _lastGenMs = new Dictionary<string, float>();
        }

        string safeLabel = string.IsNullOrEmpty(label) ? "gen" : label;
        double elapsed = Time.realtimeSinceStartupAsDouble - _genStartTime;
        float ms = (float)(elapsed * 1000.0);
        _lastGenMs[safeLabel] = ms;
        _lastGenLabel = safeLabel;

        _genStartTime = -1.0;
        IsGenerating = false;
        CurrentLabel = string.Empty;
        CurrentElapsedMs = 0f;
    }
    
    public float GetLastGenerationMs(string label)
    {
        if (_lastGenMs == null)
        {
            return -1f;
        }
        if (string.IsNullOrEmpty(label))
        {
            return -1f;
        }
        if (_lastGenMs.TryGetValue(label, out float v))
        {
            return v;
        }
        return -1f;
    }

    void Update()
    {
        _fpsAccumTime += Time.unscaledDeltaTime;
        _fpsFrames++;
        if (_fpsAccumTime >= fpsUpdateInterval)
        {
            CurrentFps = _fpsFrames / _fpsAccumTime;
            _fpsFrames = 0;
            _fpsAccumTime = 0f;
        }
        
        if (_genStartTime >= 0.0)
        {
            double elapsed = Time.realtimeSinceStartupAsDouble - _genStartTime;
            CurrentElapsedMs = (float)(elapsed * 1000.0);
        }
        else
        {
            CurrentElapsedMs = 0f;
        }
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (fpsText != null)
        {
            fpsText.SetText("{0:0.0} FPS", CurrentFps);
        }

        if (genLabelText != null)
        {
            string s = IsGenerating ? CurrentLabel : "Idle";
            genLabelText.SetText(s);
        }

        if (genTimeText != null)
        {
            if (IsGenerating)
            {
                genTimeText.SetText("Čas: " + CurrentElapsedMs + " ms");
            }
            else if (!string.IsNullOrEmpty(_lastGenLabel) && _lastGenMs != null && _lastGenMs.TryGetValue(_lastGenLabel, out float lastMs))
            {
                genTimeText.SetText("Čas: " + lastMs + " ms");
            }
        }
    }
}
