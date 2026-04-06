using UnityEngine;
using TMPro;

public class UpdateMetricsUI : MonoBehaviour
{
    public TMP_Text statsText;
    public TMP_Text infoText;

    void Start()
    {
        Metrics.Instance?.UpdateUI(statsText, infoText);
    }
}