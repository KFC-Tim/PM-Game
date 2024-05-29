using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEngine.Networking.PlayerConnection;


public class WebSocketClient : MonoBehaviour
{
    private WebSocket ws;
    public string playerName;
    public int playerIndex;
    public GameState gameState;
    public string gameId;

    void Start()
    {
        ws = new WebSocket("ws://87.106.165.86:8080");
        ws.OnMessage += OnMessageReceived;
        ws.Connect();
        playerName = "Player" + Random.Range(1, 100);
    }

    public void CreateGame()
    {
        var createMessage = new
        {
            type = "create",
            playerName = playerName
        };
        ws.Send(JsonUtility.ToJson(createMessage));
    }

    public void JoinGame(string gameId)
    {
        this.gameId = gameId;
        var joinMessage = new
        {
            type = "join",
            playerName = playerName,
            gameId = gameId
        };
        ws.Send(JsonUtility.ToJson(joinMessage));
    }

    void OnMessageReceived(object sender, MessageEventArgs e)
    {
        var data = JsonUtility.FromJson<ServerMessage>(e.Data);
        switch (data.type)
        {
            case "gameCreated":
                gameId = data.gameId;
                Debug.Log("Game created with ID: " + gameId);
                break;
            case "joined":
                gameId = data.gameId;
                Debug.Log("Joined game with ID: " + gameId);
                break;
            case "update":
                gameState = data.state;
                UpdateUI();
                break;
            case "error":
                Debug.LogError("Error: " + data.message);
                break;
            default:
                break;
        }
    }

    public void SendAnswer(int questionId, string answer)
    {
        var answerMessage = new
        {
            type = "answer",
            gameId = gameId,
            playerIndex = playerIndex,
            questionId = questionId,
            answer = answer
        };
        ws.Send(JsonUtility.ToJson(answerMessage));
    }

    void UpdateUI()
    {
        // Update your UI based on the new game state
    }

    [System.Serializable]
    public class ServerMessage
    {
        public string type;
        public string gameId;
        public GameState state;
        public string message;
    }

    [System.Serializable]
    public class GameState
    {
        public List<string> players;
        public int currentTurn;
        public List<string> questions;
        public Dictionary<string, int> scores;
    }
}
