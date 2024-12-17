using System.Collections.Generic;
using UnityEngine;

public class Audio 
{
    private static List<AudioSource> _audioSources;
    private static AudioSource _bgmSource;

    private static readonly float BgmVolume = 1f;
    private static readonly float SfxVolume = 1f;
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
        
        PlayBGM(Resources.Load<AudioClip>("audio/bgm/fairy_fountain"), 0.03f);
        PlaySFX(Resources.Load<AudioClip>("audio/sfx/wind"), 0.2f, true);
        PlaySFX(Resources.Load<AudioClip>("audio/sfx/noise"), 0.3f, true);
    }

    public static void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
    {
        _bgmSource.clip = clip;
        _bgmSource.volume = volume * BgmVolume;
        _bgmSource.loop = loop;
        _bgmSource.Play();
    }

    public static AudioSource PlaySFX(AudioClip clip, float volume = 1f, bool loop = false)
    {
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
        //! return first avalible source
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
