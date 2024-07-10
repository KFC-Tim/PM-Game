using UnityEngine;

public class Music : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    private static Music _instance;


    public static Music Instance
    {
        get { return _instance; }
    }

    public AudioSource AudioSource
    {
        get { return _audioSource; }
    }

    

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (_audioSource == null)
        {
            Debug.LogError("AudioSource ist nicht zugewiesen! Bitte weise eine AudioSource im Inspector zu.");
            return;
        }
        _audioSource.volume = CrossSceneInformation.MusicVolume;
        _audioSource.Play();
    }
}

