using System.Collections.Generic;
using UnityEngine;
using BH_Lib;
using BH_Lib.DI;
[Register]
public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<AudioSource> sfxSources = new();
    [SerializeField] private int sfxSourcePoolSize = 10;

    [Header("Volume")]
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float BGMVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;

    private void Awake()
    {
        InitSFXPool();
    }

    private void InitSFXPool()
    {
        for (int i = 0; i < sfxSourcePoolSize; i++)
        {
            var sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSources.Add(sfxSource);
        }
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = BGMVolume * MasterVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        var source = GetAvailableSFXSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = SFXVolume * MasterVolume;
            source.Play();
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
                return source;
        }

        // 만약 다 사용 중이라면, 가장 먼저 끝날 소스를 덮어쓰기
        return sfxSources[0];
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        bgmSource.volume = BGMVolume * MasterVolume;

        foreach (var sfx in sfxSources)
            sfx.volume = SFXVolume * MasterVolume;
    }

    public void MuteAll(bool mute)
    {
        bgmSource.mute = mute;
        foreach (var sfx in sfxSources)
            sfx.mute = mute;
    }
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        var source = GetAvailableSFXSource();
        if (source != null)
        {
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D 사운드
            source.minDistance = 1f;
            source.maxDistance = 20f;
            source.volume = SFXVolume * MasterVolume;
            source.PlayOneShot(clip);
        }
    }
}

