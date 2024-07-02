using UnityEngine;
using UnityEditor;
using UnityEngine.UI; 

public class MusicVolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Slider musicSlider; 

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.value = _audioSource.volume;

            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        else
        {
            Debug.LogError("Music Slider ist nicht zugewiesen! Bitte weise einen Slider im Inspector zu.");
        }
    }

    void OnMusicVolumeChanged(float volume)
    {
        if (_audioSource != null)
        {
            _audioSource.volume = volume;
        }
    }
    

}

