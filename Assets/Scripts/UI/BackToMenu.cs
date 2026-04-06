using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class BackToMenu : MonoBehaviour
    {
        public void Back()
        {
            PlayerPrefs.SetInt("returned", 1);
            SceneManager.LoadScene(0);
        }
    }
}