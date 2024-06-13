using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using static MultiplayerManager;

public class MultiplayerManager : MonoBehaviour
{
    private WebSocket websocket;
    private ClientGameState gameState;
    private GameMaster _gameMasterScript;
    private List<GameState> _gameDataQueue = new List<GameState>();
    private List<QuestionData> _questionDataQueue = new List<QuestionData>();
    private bool _isReady = true;

    void Start()
    {
        gameState = new ClientGameState();
        ConnectToServer();
    }

    async void ConnectToServer()
    {
        websocket = new WebSocket("ws://87.106.165.86:8080");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to the Server!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Disconnected from the Server");
            SwitchToMenuScene();
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            OnMessageReceived(message);
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    public void SwitchToMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public class CreateMessage
    {
        public string type;
        public string playerName;
    }

    public void CreateGame(string newPlayerName)
    {
        var createMessage = new CreateMessage
        {
            type = "create",
            playerName = newPlayerName
        };
        SendMessageToServer(JsonUtility.ToJson(createMessage));
    }

    public class JoinMessage
    {
        public string type;
        public string playerName;
        public string gameId;
    }
    public void JoinGame(string newGameId, string newPlayerName)
    {
        _gameDataQueue = new List<GameState>();
        var joinMessage = new JoinMessage
        {
            type = "join",
            playerName = newPlayerName,
            gameId = newGameId
        };
        SendMessageToServer(JsonUtility.ToJson(joinMessage));
    }

    public void SwitchToGameScene()
    {
        _isReady = false;
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Szene ist vollständig geladen
        OnGameSceneLoaded();
    }

    private void OnGameSceneLoaded()
    {
        // Hier können Sie alle Aktionen ausführen, die nach dem Laden der Szene erforderlich sind
        Debug.Log("GameScene is loaded completely.");
        SetGameMaster();
        _isReady = true;
        LoadQueues();
    }

    private void LoadQueues()
    {
        LoadQuestionDataQueue();
        LoadQuestionDataQueue();
        
        Debug.Log("Queues were loaded!");
    }
    
    private void LoadGameDataQueue()
    {
        foreach (var gameState in _gameDataQueue)
        {
            _gameMasterScript.UpdateGameState(gameState);
        }
        
        _gameDataQueue.Clear();
    }

    private void LoadQuestionDataQueue()
    {
        foreach (var question in _questionDataQueue)
        {
            DisplayQuestion(question);
        }
        
        _questionDataQueue.Clear();
    }

    async void SendMessageToServer(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            Debug.Log(message);
            var bytes = Encoding.UTF8.GetBytes(message);
            await websocket.Send(bytes);
        }
        else
        {
            Debug.LogError("Failed to send message. WebSocket is not open.");
        }
    }

    void OnMessageReceived(string message)
    {
        Debug.Log("Received message: " + message); // Log the received message for debugging
        var data = JsonUtility.FromJson<ServerMessage>(message);
        // Additional logging to inspect the deserialized data
        Debug.Log("Deserialized ServerMessage: " + JsonUtility.ToJson(data));

        if (data == null || string.IsNullOrEmpty(data.type))
        {
            Debug.LogWarning("Received message with undefined or null type.");
            return;
        }

        // Handle the message based on its type
        switch (data.type)
        {
            case "gameCreated":
                Debug.Log("Game created!");
                SwitchToGameScene();
                break;
            case "joined":
                HandleJoinGame(data);
                SwitchToGameScene();
                break;
            case "update":
                UpdateGameState(data);
                break;
            case "question":
                DisplayQuestion(data.questionData);
                break;
            case "error":
                Debug.LogError("Error: " + data.message);
                break;
            default:
                Debug.LogWarning("Unhandled message type: " + data.type);
                break;
        }
    }

    private void SetGameMaster()
    {
        GameObject gameMasterObj = GameObject.FindWithTag("GameMaster");
        if(gameMasterObj == null)
        {
           Debug.LogError("Could not find GameMaster in scene!!");
           return;
        }
        
        _gameMasterScript = gameMasterObj.GetComponent<GameMaster>();
        if (_gameMasterScript == null)
        {
            Debug.LogError("GameMaster component not found on GameMaster object!!");
        }
    }
    

    private void UpdateGameState(ServerMessage data)
    {
        if (data == null)
        {
            Debug.LogError("ServerMessage data is null");
            return;
        }

        if (data.state == null)
        {
            Debug.LogError("ServerMessage has no GameState in it");
            return;
        }

        if (!_isReady)
        {
            if (_gameDataQueue == null)
            {
                Debug.LogError("Server queque is null");
                return;
            }
            _gameDataQueue.Add(data.state);
            return;
        }

        gameState.GameState = data.state;
        _gameMasterScript.UpdateGameState(gameState.GameState);
    }

    private void HandleJoinGame(ServerMessage data)
    {
        if (data == null)
        {
            Debug.LogError("ServerMessage data is null");
            return;
        }

        if (data.state == null)
        {
            Debug.LogError("GameState in ServerMessage is null");
            return;
        }

        // Initialize the players list if it is null
        if (data.state.players == null)
        {
            data.state.players = new List<Player>();
        }

        // Initialize the scores dictionary if it is null
        if (data.state.scores == null)
        {
            data.state.scores = new Dictionary<string, int>();
        }

        gameState.GameId = data.gameId;
        gameState.GameState = data.state;

        Debug.Log("Joined Game: " + gameState.GameId);
        foreach (var player in data.state.players)
        {
            if (player == null)
            {
                Debug.LogError("Player is null in players list");
                continue;
            }
            Debug.Log("Player UUID: " + player.uuid + ", Name: " + player.name);
        }

        foreach (var score in data.state.scores)
        {
            Debug.Log("Player UUID: " + score.Key + ", Score: " + score.Value);
        }

        Debug.Log("Current Turn: " + data.state.currentTurn);
    }


    private class AnswerMessage
    {
        public string type;
        public string answer;
    }

    private void DisplayQuestion(QuestionData questionData)
    {
        if (!_isReady)
        {
            _questionDataQueue.Add(questionData);
            Debug.Log("Question queued");
            return;
        }
        _gameMasterScript.AnswerQuestion(questionData);
        Debug.Log("Question: " + questionData.question);
        for (int i = 0; i < questionData.answers.Length; i++)
        {
            Debug.Log("Answer " + i + ": " + questionData.answers[i]);
        }


        var answerMessage = new AnswerMessage
        {
            type = "answer",
            answer = questionData.answers[0] // Choose the first answer
        };
        SendMessageToServer(JsonUtility.ToJson(answerMessage));
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    [System.Serializable]
    public class ServerMessage
    {
        public string type;
        public string gameId;
        public GameState state;
        public string message;
        public QuestionData questionData;
    }

    [System.Serializable]
    public class GameState
    {
        public List<Player> players;
        public int currentTurn;
        public Dictionary<string, int> scores;
    }

    [System.Serializable]
    public class Player
    {
        public string uuid;
        public string name;
        public int playerPosition;
    }

    [System.Serializable]
    public class QuestionData
    {
        public string question;
        public string[] answers;
    }
}
