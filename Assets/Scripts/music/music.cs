using UnityEngine;

public class Music : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    private static Music _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (_audioSource == null)
        {
            Debug.LogError("AudioSource ist nicht zugewiesen! Bitte weise eine AudioSource im Inspector zu.");
            return;
        }
        _audioSource.Play();
    }
}

