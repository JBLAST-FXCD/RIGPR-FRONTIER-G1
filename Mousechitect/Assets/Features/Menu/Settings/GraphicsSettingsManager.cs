using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("Dropdown References")]
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown visualPopDropdown;

    private Resolution[] resolutions;

    private void Start()
    {
        SetupResolutionDropdown();
        displayModeDropdown.value = PlayerPrefs.GetInt("DisplayMode", 1);
        visualPopDropdown.value = PlayerPrefs.GetInt("VisualPop", 0);

        qualityDropdown.onValueChanged.AddListener(SetQuality);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        visualPopDropdown.onValueChanged.AddListener(SetVisualPopulation);
    }

    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetDisplayMode(int modeIndex)
    {
        bool isFullscreen = (modeIndex == 1);
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("DisplayMode", modeIndex);
    }

    public void SetVisualPopulation(int popIndex)
    {
        PlayerPrefs.SetInt("VisualPop", popIndex);
        Debug.Log("Visual Population Cap set to index: " + popIndex);
    }
}
