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
    public string playerName;
    public int playerIndex;
    public GameState gameState;
    public string gameId;

    void Start()
    {
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
    public void JoinGame(string gameId)
    {
        this.gameId = gameId;
        var joinMessage = new
        {
            type = "join",
            playerName = playerName,
            gameId = gameId
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


    public class QuestionData
    {
        public string question;
        public string[] answers;
    }

    private void DisplayQuestion(QuestionData questionData)
    {
        throw new NotImplementedException();
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

    void UpdateUI()
    {
        // Update your UI based on the new game state
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
        public List<PlayerInfo> players;
        public int currentTurn;

        // Dictionary mapping player UUIDs to their scores
        public Dictionary<string, int> scores;
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public string uuid;
        public string name;
    }
}