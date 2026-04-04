using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject main;
        [SerializeField]
        private GameObject secondary;

        public void Start()
        {
            if (!PlayerPrefs.HasKey("returned"))
            {
                return;
            }
            StartButton();
            PlayerPrefs.DeleteKey("returned");
        }
        
        public void StartButton()
        {
            main.SetActive(false);
            secondary.SetActive(true);
        }

        public void BackButton()
        {
            secondary.SetActive(false);
            main.SetActive(true);
        }

        public void QuitButton()
        {
            Application.Quit();
        }

        public void CellularAutomata()
        {
            SceneManager.LoadScene(1);
        }

        public void LSystem()
        {
            SceneManager.LoadScene(2);
        }

        public void PerlinNoise()
        {
            SceneManager.LoadScene(3);
        }

        public void WaveFunctionCollapse()
        {
            SceneManager.LoadScene(4);
        }
    }
}