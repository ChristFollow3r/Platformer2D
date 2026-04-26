using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class Menu : MonoBehaviour
    {
        public void MenuButton()
        {
            SceneManager.LoadScene(0);
        }
        public void PlayButton()
        {
            SceneManager.LoadScene(1);
        }

        public void PlatformerButton()
        {
            SceneManager.LoadScene(2);
        }

        public void CreditsButton()
        {
            SceneManager.LoadScene(3);
        }

        public void QuitButton()
        {
            Application.Quit();
        }
    }
}
