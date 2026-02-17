using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button NewGameButton;
    public Button LoadGameButton;
    public Button SettingsButton;
    public Button QuitButton;

    private void Start()
    {
        if (NewGameButton != null)
        {
            NewGameButton.onClick.AddListener(OnNewGameClicked);
        }

        if (LoadGameButton != null)
        {
            LoadGameButton.onClick.AddListener(OnLoadGameClicked);
        }

        if (SettingsButton != null)
        {
            SettingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (QuitButton != null)
        {
            QuitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnNewGameClicked()
    {
        Debug.Log("New Game Loading.");
        SceneManager.LoadScene(1);
    }

    private void OnLoadGameClicked()
    {
        Debug.Log("Load Game Menu Opened.");
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings Menu Opened.");
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quitting Game.");
        Application.Quit();
    }
}
