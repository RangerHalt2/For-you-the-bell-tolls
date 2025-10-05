// Created By: Ryan Lupoli
// Manages Audio SLiders 
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    #region Variables
    [Header("Settings")]
    [Tooltip("Reference to Sound Slider being used to manage volume.")]
    [SerializeField] Slider soundSlider;
    [Tooltip("Reference to Audio Mixer.")]
    [SerializeField] AudioMixer masterMixer;
    [Tooltip("Determines which Audio Group is being managed by the object.")]
    public MixerGroup mixerGroup;

    private string savedVolumeGroup;
    private string volumeGroup;

    // Each case represents a group in the audio mixer
    public enum MixerGroup
    {
        Master,
        UISFX,
        GameSFX,
        Music
    }

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeAudioManager();

        // If a sound Slider has been set
        if (soundSlider != null)
        {
            // Get the float of the saved volume
            SetVolume(PlayerPrefs.GetFloat(savedVolumeGroup, 100));
        }
        // Otherwise print a warning message in console
        else
        {
            Debug.LogWarning("Audio Manager: No Sound Slider Set. Audio will be unable to be altered in this scene.");
        }
    }

    // Sets the volume of the assigned mixer to a specific value
    public void SetVolume(float volume)
    {
        // If volume is less than 1, mute the audio
        if (volume < 1)
        {
            volume = .001f;
        }

        // Reset the position of the slider
        RefreshSlider(volume);
        // Update the saved volume for the gorup
        PlayerPrefs.SetFloat(savedVolumeGroup, volume);
        // Update the volume on the master mixer
        masterMixer.SetFloat(volumeGroup, Mathf.Log10(volume / 100) * 20f);
    }

    // Sets the volume based off of the value of a slider
    public void SetVolumeFromSlider()
    {
        SetVolume(soundSlider.value);
    }

    // Resets the slider to a specified value
    public void RefreshSlider(float value)
    {
        soundSlider.value = value;
    }

    // Set the savedVolumeGroup and volumeGroup strings in accordance with the selected mixerGroup
    // If new mixer groups are added, create an addition case to support them
    public void InitializeAudioManager()
    {
        switch (mixerGroup)
        {
            // Master 
            case MixerGroup.Master:
                savedVolumeGroup = "SavedMasterVolume";
                volumeGroup = "MasterVolume";
                break;
            // UI SFX
            case MixerGroup.UISFX:
                savedVolumeGroup = "SavedUISFXVolume";
                volumeGroup = "UISFXVolume";
                break;
            // Game SFX
            case MixerGroup.GameSFX:
                savedVolumeGroup = "SavedGameSFXVolume";
                volumeGroup = "GameSFXVolume";
                break;
            // Music
            case MixerGroup.Music:
                savedVolumeGroup = "SavedMusicVolume";
                volumeGroup = "MusicVolume";
                break;
            // Deafault Case. Should never be called
            default:
                savedVolumeGroup = "SavedMasterVolume";
                volumeGroup = "MasterVolume";
                break;

        }
    }
}
