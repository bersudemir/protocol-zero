
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSceneController : MonoBehaviour
{
    //ana menu butonu
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    //cýkýs butonu
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
