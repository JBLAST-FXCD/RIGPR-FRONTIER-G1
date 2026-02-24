using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("UI Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolumePref", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolumePref", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolumePref", 1f);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
    }

    public void SetMasterVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;

        mainMixer.SetFloat("MasterVolume", dbValue);

        PlayerPrefs.SetFloat("MasterVolumePref", sliderValue);
    }

    public void SetMusicVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat("MusicVolume", dbValue);
        PlayerPrefs.SetFloat("MusicVolumePref", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        mainMixer.SetFloat("SFXVolume", dbValue);
        PlayerPrefs.SetFloat("SFXVolumePref", sliderValue);
    }
}
