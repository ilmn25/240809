using System.Collections.Generic;
using UnityEngine;

public class AudioSingleton : MonoBehaviour
{
    //TODO 
    public AudioClip BGM;
    public AudioClip WIND;
    public AudioClip NOISE;

    private static List<AudioSource> _audioSources;
    private static AudioSource _BGMSource;

    public static float BGMVOLUME = 1f;
    public static float SFXVOLUME = 1f;
    public int POOLSIZE = 10;

    private void Start()
    {
        _BGMSource = gameObject.AddComponent<AudioSource>(); 

        // Initialize the audio source pool
        _audioSources = new List<AudioSource>();
        for (int i = 0; i < POOLSIZE; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            _audioSources.Add(newSource);
        }
        
        // PlayBGM(BGM, 0.1f);
        PlaySFX(WIND, 0.2f, true);
        PlaySFX(NOISE, 0.3f, true);
    }

    public static void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
    {
        _BGMSource.clip = clip;
        _BGMSource.volume = volume * BGMVOLUME;
        _BGMSource.loop = loop;
        _BGMSource.Play();
    }

    public static AudioSource PlaySFX(AudioClip clip, float volume = 1f, bool loop = false)
    {
        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource != null)
        {
            availableSource.clip = clip;
            availableSource.volume = volume * SFXVOLUME;
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
