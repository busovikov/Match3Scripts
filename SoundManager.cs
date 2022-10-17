using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SoundManager : MonoBehaviour
{
    public GameObject volumeSlider;
    public GameObject musicButton;
    public GameObject volumeButton;

    public AudioClip popSound;
    public AudioClip popupDropSound;
    public AudioClip[] Themes;

    AudioSource popSoundSource;
    AudioSource popupDropSoundSource;
    AudioSource ThemeSource;

    [Range(0f, 1f)]
    public float volumeMusic = 0.5f;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(1f, 3f)]
    public float pitch = 1f;
    public bool mute = false;
    
    private const float sliderDuration = 3f;
    private float sliderTimer = 0;

    private void Awake()
    {
        popSoundSource = gameObject.AddComponent<AudioSource>();
        popupDropSoundSource = gameObject.AddComponent<AudioSource>();
        ThemeSource = gameObject.AddComponent<AudioSource>();
    }

    void InitSoundSources()
    {
        Config.LoadBool("Mute", out mute, mute);
        Config.LoadFloat("volume", out volume, volume);
        Config.LoadFloat("volumeMusic", out volumeMusic, volumeMusic);
        
        if(volumeSlider != null)
            volumeSlider.GetComponent<Slider>().SetValueWithoutNotify(volume);
        if(musicButton != null)
            musicButton.GetComponent<CheckButton>().SetOn(!mute);
        if(volumeButton != null)
            volumeButton.GetComponent<CheckButton>().SetOn(volume > 0);

        popSoundSource.clip = popSound;
        popSoundSource.volume = volume;
        popSoundSource.pitch = pitch;

        popupDropSoundSource.clip = popupDropSound;
        popupDropSoundSource.volume = volume;
        popupDropSoundSource.pitch = pitch;

        ThemeSource.volume = volumeMusic;
        ThemeSource.pitch = pitch;
        ThemeSource.loop = true;
    }

    public void OnMuteChanged()
    {
        mute = !mute;
        if (musicButton.GetComponent<CheckButton>().CheckIfOnAndSwitch())
        {
            volumeMusic = 0;
            ThemeSource.volume = volumeMusic;
        }
        else
        {
            volumeMusic = volume;
            ThemeSource.volume = volumeMusic;
            
        }
        Config.SaveFloat("volumeMusic", volumeMusic);
        Config.SaveBool("Mute",mute);
    }

    public void OnVolumeChanged()
    {
        volume = volumeSlider.GetComponent<Slider>().value;
        volumeButton.GetComponent<CheckButton>().SetOn(volume > 0);
        if (!mute)
        {
            volumeMusic = volume;
            ThemeSource.volume = volume;
            Config.SaveFloat("volumeMusic", volume);
        }
        popSoundSource.volume = volume;
        popupDropSoundSource.volume = volume;
        Config.SaveFloat("volume", volume);
        ShowVolumeSlider();
    }

    public void OnTileDeleted(Tile sender, LevelGrid.Tile type)
    {
        PlayPop();
    }
        private void Start()
    {
        InitSoundSources();
        var scene = SceneManager.GetActiveScene();
        if (scene.buildIndex > 0)
        {
            PlayRandomTheme();
        }
        else
        {
            PlayMainTheme();
        }
    }

    public void PlayPop()
    {
        popSoundSource.Play();
    }

    public void PlayPopupSound()
    {
        popupDropSoundSource.Play();
    }

    public void PlayMainTheme()
    {
        ThemeSource.clip = Themes[0];
        ThemeSource.Play();
    }

    public void PlayRandomTheme()
    {
        ThemeSource.clip = Themes[UnityEngine.Random.Range(1, Themes.Length)];
        ThemeSource.Play();
    }

    public void ShowVolumeSlider()
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetActive(true);
            sliderTimer = sliderDuration;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(sliderTimer>0)
            sliderTimer -= Time.deltaTime;
        else if(volumeSlider != null && volumeSlider.activeSelf)
        {
            volumeSlider.SetActive(false);
        }
    }
}
