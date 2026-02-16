using ImGuiNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public static AudioHandler instance;
    public AudioSource music_track_1;
    public AudioSource music_track_2;
    public AudioSource music_track_3;

    public bool is_music_playing = false;

    private void Start()
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

    private void Update()
    {
        MusicPlaying();
    }

    public void MusicPlaying()
    {
        if (!AudioHandler.instance.music_track_1.isPlaying && !AudioHandler.instance.music_track_2.isPlaying && !AudioHandler.instance.music_track_3.isPlaying)
        {
            is_music_playing = false;
        }
        else
        {
            is_music_playing = true;
        }
    }
}
