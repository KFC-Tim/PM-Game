using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GameLobby : MonoBehaviour
{
    [SerializeField] private GameMaster _gameMaster;
    [SerializeField] private GameObject _startCanvas;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _addPlayer;
    private int players = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_gameMaster == null)
        {
            Debug.LogError("ERROR GameMaster not set in GameLobby");
            return;
        }
        
        if (_startCanvas == null)
        {
            Debug.LogError("ERROR StartCanvas not set in GameLobby");
            _gameMaster.StartGame(4);
        }

        _startCanvas.SetActive(true);

        _startButton.onClick.AddListener(() => StartButtonClick());
        _addPlayer.onClick.AddListener(() => CheckAndAddPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool CheckAndAddPlayer()
    {
        if (players < 4)
        {
            players++;
            if (players == 4)
            {
                _addPlayer.image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
            
            return true;
        }

        

        return false;
    }

    void StartButtonClick()
    {
        _startCanvas.SetActive(false);
        _gameMaster.StartGame(players);
    }
}
