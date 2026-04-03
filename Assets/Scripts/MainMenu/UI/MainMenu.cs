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

        public void CaButton()
        {
            SceneManager.LoadScene(1);
        }

        public void LsButton()
        {
            SceneManager.LoadScene(2);
        }

        public void PnButton()
        {
            SceneManager.LoadScene(3);
        }

        public void WfcButton()
        {
            SceneManager.LoadScene(4);
        }
    }
}