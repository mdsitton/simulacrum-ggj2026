
using UnityEngine;

public class RestartGame : MonoBehaviour
{
    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void Continue()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}