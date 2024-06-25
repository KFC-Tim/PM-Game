using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using MiniJSON;

public class MultiplayerManager : MonoBehaviour
{
    private WebSocket websocket;
    private ClientGameState _gameState = new ClientGameState();
    private GameMaster _gameMasterScript;
    private List<GameState> _gameDataQueue = new List<GameState>();
    private List<QuestionData> _questionDataQueue = new List<QuestionData>();
    private int _playerCount = 0;
    private bool _isReady = true;
    private static MultiplayerManager Instance; 

    void Start()
    {
        _gameState = new ClientGameState();
        ConnectToServer();
    }
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static MultiplayerManager GetInstance()
    {
        return Instance;
    }

    public string GetGameId()
    {
        if (_gameState == null)
        {
            Debug.LogError("GameState is not set in MultiplayerManager");
            return "ERROR";
        }

        return _gameState.GameId;
    }

    async void ConnectToServer()
    {
        websocket = new WebSocket("wss://manager-rumble.de:8080");

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

    [System.Serializable]
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
        Debug.Log("Serializing JSON CreateGame message");
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
    
    public void SwitchToLobbyScene()
    {
        _isReady = false;
        SceneManager.LoadSceneAsync("LobbyScene");
    }

    public void SwitchToGameScene()
    {
        _isReady = false;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        OnGameSceneLoaded();
        //StartCoroutine(LoadGameScene());
    }

    public void StartGame()
    {
        SendMessageToServer("{\"type\": \"start\", \"gameId\": \""+ _gameState.GameId +"\"}");
        Debug.Log("Game Started");
        SwitchToGameScene();
    }

    public int GetPlayerCount()
    {
        return _playerCount;
    }

    private void OnGameSceneLoaded()
    {
        Debug.Log("GameScene is loaded completely.");
        //StartCoroutine(SetGameMasterWhenReady());
    }

    private void LoadQueues()
    {
        LoadGameDataQueue();
        LoadQuestionDataQueue();
        
        Debug.Log("Queues were loaded!");
    }
    
    private void LoadGameDataQueue()
    {
        foreach (var gameState in _gameDataQueue)
        {
            _gameMasterScript.UpdateGameState(gameState);
            Debug.Log("Loading: " + JsonUtility.ToJson(gameState));
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
        ServerMessage data;
        try
        {
            data = DeserializeServerMessage(message);
            //data = (ServerMessage) Json.Deserialize(message);
            // Additional logging to inspect the deserialized data
            if (data == null)
            {
                Debug.LogWarning("Deserialization resulted in a null object.");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        

        if (string.IsNullOrEmpty(data.type))
        {
            Debug.LogWarning("Received message with undefined type.");
            return;
        }

        // Handle the message based on its type
        switch (data.type)
        {
            case "gameCreated":
                Debug.Log("Game created!");
                SwitchToLobbyScene();
                break;
            case "joined":
                HandleJoinGame(data);
                SwitchToLobbyScene();
                break;
            case "update":
                UpdateGameState(data);
                break;
            case "question":
                Debug.Log("Question!");
                DisplayQuestion(data.questionData);
                break;
            case "game_end":
                Debug.Log("Game End!");
                break;
            case "start":
                Debug.Log("Game Starts!");
                GameStarts();
                break;
            case "error":
                Debug.LogError("Error: " + data.message);
                break;
            default:
                Debug.LogWarning("Unhandled message type: " + data.type);
                break;
        }
    }
    
    private ServerMessage DeserializeServerMessage(string json)
    {
        Debug.Log("Received JSON: " + json);
        ServerMessage serverMessage = null;

        try
        {
            // Deserialisiere die Nachricht in ein Objekt
            serverMessage = JsonUtility.FromJson<ServerMessage>(json);
            Debug.Log("Deserialized ServerMessage: " + JsonUtility.ToJson(serverMessage, true));

            // Parsiere das JSON manuell mit MiniJSON, um flexibler zu sein
            var jsonObject = Json.Deserialize(json) as Dictionary<string, object>;
            if (jsonObject != null && jsonObject.ContainsKey("state"))
            {
                var stateObject = jsonObject["state"] as Dictionary<string, object>;
                if (stateObject != null && stateObject.ContainsKey("scores"))
                {
                    var scoresObject = stateObject["scores"] as Dictionary<string, object>;
                    if (scoresObject != null)
                    {
                        // Convert scores dictionary from object to int
                        var scores = scoresObject.ToDictionary(kvp => kvp.Key, kvp => Convert.ToInt32(kvp.Value));
                        serverMessage.state.scores = scores;

                        // Debug-Log für die deserialisierten Scores
                        foreach (var score in serverMessage.state.scores)
                        {
                            Debug.Log($"Player: {score.Key}, Score: {score.Value}");
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to deserialize scores");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception during deserialization: " + ex.Message);
            Debug.LogError("StackTrace: " + ex.StackTrace);
        }

        // Debugging-Ausgaben um sicherzustellen, dass message korrekt deserialisiert wurde
        if (serverMessage == null)
        {
            Debug.LogError("Deserialization of ServerMessage failed.");
            return null;
        }

        // Debugging-Ausgabe für das Scores-Dictionary
        if (serverMessage.state != null && serverMessage.state.scores != null)
        {
            Debug.Log("Deserialized scores: " + string.Join(", ", serverMessage.state.scores.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
        }
        else
        {
            Debug.LogWarning("Message's GameState or scores are null/empty");
        }

        return serverMessage;
    }
    

    private void GameStarts()
    {
        SwitchToGameScene();
    }

    private IEnumerator SetGameMasterWhenReady()
    {
        while (!CrossSceneInformation.GameMasterLoaded)
        {
            Debug.Log("Waiting for GameMaster to be loaded...");
            yield return new WaitForSeconds(1.0f);
        }
        Debug.Log("Game Master initialized");
        
        _gameMasterScript = CrossSceneInformation.GameMasterInstance;
        if (_gameMasterScript != null)
        { 
            _isReady = true;
            LoadQueues();
            Debug.Log("GameMaster is set and queues are loaded.");
        }
        else
        { 
            Debug.LogError("GameMaster component not found on GameMaster object after initialization.");
        }
        
    }

    public void SetGameMaster(GameMaster gameMaster)
    {
        _gameMasterScript = gameMaster;
        _isReady = true;
        LoadQueues();
        Debug.Log("GameMaster is set and queues are loaded.");
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

        if (data.state.scores == null)
        {
            Debug.LogError("No Scores  in data");
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

        _playerCount = data.state.scores.Count;

        _gameState.GameState = data.state;
        Debug.Log(_gameState);
        Debug.Log("GameState:  "+ JsonUtility.ToJson(_gameState.GameState));
        _gameMasterScript.UpdateGameState(_gameState.GameState);
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
            data.state = new GameState();
        }
        
        if (data.state.players == null)
        {
            data.state.players = new List<Player>();
        }

        // Initialize the scores dictionary if it is null
        if (data.state.scores == null)
        {
            data.state.scores = new Dictionary<string, int>();
        }

        if (data.playerNumber == null)
        {
            data.playerNumber = 1;
        }

        _gameState.GameId = data.gameId;
        _gameState.GameState = data.state;

        Debug.Log("Joined Game: " + _gameState.GameId);
        _playerCount = data.playerNumber;
        foreach (var player in data.state.players)
        {
            if (player == null)
            {
                Debug.LogError("Player is null in players list");
                continue;
            }

            _playerCount++;
            Debug.Log("Player UUID: " + player.uuid + ", Name: " + player.name);
        }

        foreach (var score in data.state.scores)
        {
            Debug.Log("Player UUID: " + score.Key + ", Score: " + score.Value);
        }

        Debug.Log("Current Turn: " + data.state.currentTurn);
    }

    private void GameEnd(string winner)
    {
        Debug.Log("Winner is "+ winner);
    }


    [System.Serializable]
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

        Debug.Log("Question will be shown now!");
        try
        {
            _gameMasterScript.AnswerQuestion(questionData, (answer) =>
            {
                Debug.Log("Received answer: " + answer);
                var answerMessage = new AnswerMessage
                {
                    type = "answer",
                    answer = answer 
                };
                SendMessageToServer(JsonUtility.ToJson(answerMessage));
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
            websocket = null;
        }
    }

    [System.Serializable]
    public class ServerMessage
    {
        public string type;
        public string gameId;
        public GameState state;
        public string message;
        public QuestionData questionData;
        public int playerNumber;
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
