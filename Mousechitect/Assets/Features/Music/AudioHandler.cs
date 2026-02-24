using ImGuiNET;
using System.Collections;
using System.Collections.Generic;
using UImGui;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public static AudioHandler instance;
    public AudioSource music_source;

    public AudioClip[] music_tracks;

    public bool is_music_playing => music_source != null && music_source.isPlaying;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(int track_index)
    {
        if (track_index <0 || track_index >= music_tracks.Length)
        {
            DebugWindow.LogToConsole($"Invalid track index: {track_index}", true);
            return;
        }

        music_source.clip = music_tracks[track_index];
        music_source.Play();

        DebugWindow.LogToConsole($"Playing music track: {music_tracks[track_index].name}", true);
    }

    public void StopMusic()
    {
        if (is_music_playing)
        {
            music_source.Stop();
        }
    }
}
