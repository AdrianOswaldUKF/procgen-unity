using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class BackToMenu : MonoBehaviour
    {
        public void Back()
        {
            if (Metrics.Instance != null && !Metrics.Instance.CanGenerate)
                return;
            
            Metrics.Instance?.ExportCsv();
            PlayerPrefs.SetInt("returned", 1);
            SceneManager.LoadScene(0);
        }
    }
}