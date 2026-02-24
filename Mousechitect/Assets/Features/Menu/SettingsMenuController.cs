using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Settings Pages")]
    public GameObject audioPage;
    public GameObject graphicsPage;
    public GameObject controllerPage;
    public GameObject keyboardPage;

    [Header("Tab Buttons")]
    public Button audioTab;
    public Button graphicsTab;
    public Button controllerTab;
    public Button keyboardTab;

    private void Start()
    {
        audioTab.onClick.AddListener(OpenAudio);
        graphicsTab.onClick.AddListener(OpenGraphics);
        controllerTab.onClick.AddListener(OpenController);
        keyboardTab.onClick.AddListener(OpenKeyboard);

        OpenAudio();
    }

    public void OpenAudio()
    {
        CloseAllPages();
        audioPage.SetActive(true);
    }

    public void OpenGraphics()
    {
        CloseAllPages();
        graphicsPage.SetActive(true);
    }

    public void OpenController()
    {
        CloseAllPages();
        controllerPage.SetActive(true);
    }

    public void OpenKeyboard()
    {
        CloseAllPages();
        keyboardPage.SetActive(true);
    }

    private void CloseAllPages()
    {
        audioPage.SetActive(false);
        graphicsPage.SetActive(false);
        controllerPage.SetActive(false);
        keyboardPage.SetActive(false);
    }
}
