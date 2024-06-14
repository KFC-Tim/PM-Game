using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MultiplayerManager;

public class ClientGameState : MonoBehaviour
{

    private string playerName = "";
    private int playerIndex = 0;
    private GameState gameState = null;
    private string gameId = "";
    private Camera playerCamera;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string PlayerName
    {
        get { return playerName; }
        set { playerName = value; }
    }

    public int PlayerIndex
    {
        get { return playerIndex; }
        set { playerIndex = value; }
    }

    public GameState GameState
    {
        get { return gameState; }
        set { gameState = value; }
    }

    public string GameId
    {
        get { return gameId; }
        set { gameId = value; }
    }
    
    public Camera PlayerCamera
    {
        get { return playerCamera; }
        set { playerCamera = value; }
    }
}
