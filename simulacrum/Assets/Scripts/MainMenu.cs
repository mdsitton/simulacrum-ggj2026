
using UnityEngine.SceneManagement;
using UnityEngine;

class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("level-scene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}