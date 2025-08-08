using System.Collections.Generic;
using UnityEngine;

public class Audio
{
    private static List<AudioSource> _audioSources;
    private static AudioSource _bgmSource;

    private static readonly float BgmVolume = 1f;
    private static readonly float SfxVolume = 0.5f;
    private static readonly int PoolSize = 12;

    public static void Initialize()
    {
        GameObject audioManager = new GameObject("Audio");
        _bgmSource = audioManager.AddComponent<AudioSource>();

        _audioSources = new List<AudioSource>();
        for (int i = 0; i < PoolSize; i++)
        {
            AudioSource newSource = audioManager.AddComponent<AudioSource>();
            _audioSources.Add(newSource);
        }

        PlayBGM("fairy_fountain", 0.4f);
        PlaySFX("wind", 0.2f, true);
        PlaySFX("noise", 0.3f, true);
    }

    public static void PlayBGM(string id, float volume = 1f, bool loop = true)
    {
        AudioClip clip = Cache.LoadAudioClip($"audio/bgm/{id}");
        if (clip == null) return;

        _bgmSource.clip = clip;
        _bgmSource.volume = volume * BgmVolume;
        _bgmSource.loop = loop;
        _bgmSource.Play();
    }

    public static AudioSource PlaySFX(string id, float volume = 1f, bool loop = false)
    {
        if (id == "") return null;
        AudioClip clip = Cache.LoadAudioClip($"sfx/{id}");
        if (clip == null) return null;

        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource)
        {
            availableSource.clip = clip;
            availableSource.volume = volume * SfxVolume;
            availableSource.loop = loop;
            availableSource.Play();
        }
        return availableSource;
    }

    public static void StopSFX(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    private static AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in _audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }
}
