using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Text;
using System.Collections.Generic;
using System;

public class MultiplayerManager : MonoBehaviour
{
    private NetworkDriver driver;
    private NetworkConnection connection;

    private ClientGameState gameState;

    void Start()
    {
        gameState = new ClientGameState();
        driver = NetworkDriver.Create();
        connection = default(NetworkConnection);
        var endpoint = NetworkEndpoint.Parse("87.106.165.86", 8080);
        connection = driver.Connect(endpoint);
    }

    void Update()
    {
        driver.ScheduleUpdate().Complete();

        if (!connection.IsCreated)
        {
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                // Read incoming data length and bytes
                int length = stream.Length;
                var buffer = new NativeArray<byte>(length, Allocator.Temp);
                stream.ReadBytes(buffer);


                // Convert byte array to string and

                var message = Encoding.UTF8.GetString(buffer.ToArray());
                OnMessageReceived(message);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection = default(NetworkConnection);
            }
        }
    }

    // Method to create a game
    public void CreateGame(string newPlayerName)
    {
        var createMessage = new
        {
            type = "create",
            playerName = newPlayerName
        };
        SendMessage(JsonUtility.ToJson(createMessage));
    }

    // Method to join an existing game
    public void JoinGame(string newGameId, string newPlayerName)
    {
        var joinMessage = new
        {
            type = "join",
            playerName = newPlayerName,
            gameId = newGameId
        };
        SendMessage(JsonUtility.ToJson(joinMessage));
    }

    // Handle received messages
    void OnMessageReceived(string message)
    {
        var data = JsonUtility.FromJson<ServerMessage>(message);
        switch (data.type)
        {
            case "gameCreated":
                Debug.Log("Game created!");
                break;
            case "joined":
                HandleJoinGame(data);
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

    private void UpdateGameState(ServerMessage data)
    {
        if (data == null)
        {
            Debug.LogError("ServerMessage data is null");
            return;
        }

        gameState.GameState = data.state;
    }

    private void HandleJoinGame(ServerMessage data)
    {
        if (data == null)
        {
            Debug.LogError("ServerMessage data is null");
            return;
        }

        gameState.GameId = data.gameId;
        gameState.GameState = data.state;

        Debug.Log("Joined Game: " + gameState.GameId);
        foreach (var player in data.state.players)
        {
            Debug.Log("Player UUID: " + player.uuid + ", Name: " + player.name);
        }

        foreach (var score in data.state.scores)
        {
            Debug.Log("Player UUID: " + score.Key + ", Score: " + score.Value);
        }

        Debug.Log("Current Turn: " + data.state.currentTurn);
    }

    public class QuestionData
    {
        public string question;
        public string[] answers;
    }

    private void DisplayQuestion(QuestionData questionData)
    {
        throw new NotImplementedException();
        //richtige Antwort auswählen und dann die ausgewählte Antwort als String senden
        /*var answerMessage = new
        {
            answer = "Die Antwort hier hin"
        };
        SendMessage(JsonUtility.ToJson(answerMessage));
        */
    }

    new
        void SendMessage(string message)
    {
        DataStreamWriter writer;
        if (driver.BeginSend(connection, out writer) == 0)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            writer.WriteInt(bytes.Length);
            using (var nativeArray = new NativeArray<byte>(bytes, Allocator.Temp))
            {
                writer.WriteBytes(nativeArray);
            }
            var status = driver.EndSend(writer);
            if (status < 0)
            {
                Debug.LogError("Failed to send message: " + status);
            }
        }
        else
        {
            Debug.LogError("Failed to begin send.");
        }
    }

    void OnDestroy()
    {
        driver.Dispose();
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

        // Dictionary mapping player UUIDs to their scores
        public Dictionary<string, int> scores; // Aktuell hat jeder Spieler nur eine Spielfigur wird noch geändert!!!
    }

    [System.Serializable]
    public class Player
    {
        public string uuid;
        public string name;
        public int playerPosition; // ob erster zweiter, dritter oder vierter spieler -> wichtig für den GameMaster
    }
}