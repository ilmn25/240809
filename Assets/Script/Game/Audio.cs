using System.Collections.Generic;
using UnityEngine;

public class Audio
{
    private static readonly Dictionary<AudioSource, float> BgmBase = new Dictionary<AudioSource, float>();
    private static readonly Dictionary<AudioSource, float> AmbienceBase = new Dictionary<AudioSource, float>();
    private static readonly Dictionary<AudioSource, float> SfxBase = new Dictionary<AudioSource, float>();

    private static readonly int PoolSize = 12;
    private static readonly int AmbiencePoolSize = 4;

    private static readonly Dictionary<SfxID, float> Volume = new Dictionary<SfxID, float>
    {
        { SfxID.HitMetal, 0.3f },
        { SfxID.HitStone, 4f },
        { SfxID.Footsteps1, 0.4f },
        { SfxID.Footsteps2, 0.4f },
        { SfxID.Text, 0.3f },
        { SfxID.Sword, 0.2f },
        { SfxID.Wind, 0.7f },
    };

    public static void Initialize()
    {
        GameObject audioManager = new GameObject("Audio");
        BgmBase[audioManager.AddComponent<AudioSource>()] = 1f;

        for (int i = 0; i < AmbiencePoolSize; i++)
        {
            AmbienceBase.Add(audioManager.AddComponent<AudioSource>(), 1f);
        }

        for (int i = 0; i < PoolSize; i++)
        {
            SfxBase.Add(audioManager.AddComponent<AudioSource>(), 1f);
        }

        PlayBGM("FairyFountain", 0.2f);
        PlayAmbience(SfxID.Wind);
        PlayAmbience(SfxID.Noise);
    }

    public static void PlayBGM(string id, float volume = 1f, bool loop = true)
    {
        AudioClip clip = Cache.LoadAudioClip($"BGM/{id}");
        if (clip == null) return;

        AudioSource source = GetBgmSource();
        if (source == null) return;
        BgmBase[source] = volume;
        source.clip = clip;
        source.volume = volume * Settings.Inst.BgmVolume;
        source.loop = loop;
        source.Play();
    }

    public static void PlayAmbience(SfxID id, bool loop = true)
    {
        AudioClip clip = Cache.LoadAudioClip($"SFX/{id}");
        if (!clip) return;

        float volume = BaseVolume(id);
        AudioSource availableSource = GetAvailableSource(AmbienceBase);
        if (availableSource)
        {
            AmbienceBase[availableSource] = volume;
            availableSource.clip = clip;
            availableSource.volume = volume * Settings.Inst.AmbienceVolume;
            availableSource.loop = loop;
            availableSource.Play();
        }
    }

    public static AudioSource PlaySFX(SfxID id, bool loop = false)
    {
        AudioClip clip = Cache.LoadAudioClip($"SFX/{id}");
        if (!clip) return null;

        float volume = BaseVolume(id);
        AudioSource availableSource = GetAvailableSource(SfxBase);
        if (availableSource)
        {
            SfxBase[availableSource] = volume;
            availableSource.clip = clip;
            availableSource.volume = volume * Settings.Inst.SfxVolume;
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
            SfxBase.Remove(audioSource);
        }
    }

    public static void ApplyVolumeSettings()
    {
        ApplyPool(BgmBase, Settings.Inst.BgmVolume);
        ApplyPool(AmbienceBase, Settings.Inst.AmbienceVolume);
        ApplyPool(SfxBase, Settings.Inst.SfxVolume);
    }

    private static void ApplyPool(Dictionary<AudioSource, float> bases, float setting)
    {
        foreach (KeyValuePair<AudioSource, float> pair in bases)
        {
            if (pair.Key != null && pair.Key.isPlaying)
            {
                pair.Key.volume = pair.Value * setting;
            }
        }
    }

    private static float BaseVolume(SfxID id)
    {
        return Volume.TryGetValue(id, out float v) ? v : 1f;
    }

    private static AudioSource GetBgmSource()
    {
        foreach (AudioSource source in BgmBase.Keys)
        {
            return source;
        }
        return null;
    }

    private static AudioSource GetAvailableSource(Dictionary<AudioSource, float> sources)
    {
        foreach (AudioSource source in sources.Keys)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }
}
