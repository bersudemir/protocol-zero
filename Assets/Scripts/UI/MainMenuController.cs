
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject introPanel;

  private void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (introPanel != null)
            introPanel.SetActive(false);
    }

    // oyuna basla butonu
    public void ShowIntro()
    {
        if (menuPanel == null || introPanel == null)
        {
            Debug.LogError("Menu panel referanslari eksik.");
            return;
        }

        menuPanel.SetActive(false);
        introPanel.SetActive(true);
    }

    // göreve baþla butonu
    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    // çýkýþ butonu
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
