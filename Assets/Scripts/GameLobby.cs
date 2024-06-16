using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GameLobby : MonoBehaviour
{
    [SerializeField] private MultiplayerManager _multiplayerManager;
    [SerializeField] private GameObject _startCanvas;
    [SerializeField] private GameObject _currentPlayerText;
    [SerializeField] private Button _startButton;
    private int players = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        _multiplayerManager = CrossSceneInformation.MultiplayerManager;
        if (_multiplayerManager == null)
        {
            Debug.LogError("ERROR MultiplayerManager not set in GameLobby");
            return;
        }
        
        if (_startCanvas == null)
        {
            Debug.LogError("ERROR StartCanvas not set in GameLobby");
        }

        _startCanvas.SetActive(true);

        _startButton.onClick.AddListener(() => StartButtonClick());
    }

    // Update is called once per frame
    void Update()
    {
        players = _multiplayerManager.GetPlayerCount();
    }

    void UpdateCurrentPlayerText()
    {
        var text = _currentPlayerText.GetComponent<TextMeshProUGUI>();
        text.SetText("Current Players: " + players);
    }
    

    void StartButtonClick()
    {
        Debug.Log("Start Button in Lobby Clicked!!");
        _startCanvas.SetActive(false);
        _multiplayerManager.StartGame();
    }
}
