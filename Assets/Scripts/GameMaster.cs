using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour, IGameController
{
    public InventoryManager inventoryManager;
    [SerializeField] private QuestionsController questionsController;
    public FieldEventController fieldEventController;

    [SerializeField] private TMP_Text _killText;
    [SerializeField] private GameObject _killCanvas;
    [SerializeField] private AudioClip _killAudio;
    [SerializeField] private AudioClip _killedAudio;
    
    [SerializeField] private AudioSource _audioSource;
    
    [SerializeField] private GameObject[] playerPiecePrefabs = new GameObject[4];
    public List<PlayerPiece> playerPieces;
    public int[] playerRounds = new int[4];
    

    [SerializeField] private Board board;

    [SerializeField] private int totalPlayers = 4; // required to have 4 players

    public int currentPlayerIndex = 0;

    private bool gameIsOver = false;
    private bool[] skipQuestion = {false, false, false, false};
    private bool hasSelected = false;

    
    
    public static GameMaster Instance { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameMaster started!!");
        CrossSceneInformation.SetGameMasterLoaded(true);
        CrossSceneInformation.SetGameMasterInstance(this);
        InitializePlayerPieces();
        questionsController.SetGameController(this);
        
        if(board == null){
            Debug.Log("Board component not foun in the scene");
            return;
        }

        //InitializePlayerPieces();

        // maybe here the random or by join the lobby
        currentPlayerIndex = 0;
        gameIsOver = false;
    }
    
    void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    public void StartGame(int totalPlayers)
    {
        this.totalPlayers = totalPlayers;
        inventoryManager.ShowInventory();
        
        // Starts the game
        AtTurn();
    }

    public void UpdateGameState(MultiplayerManager.GameState gameState)
    {
        int i = 0;
        Debug.Log("Updating Game State");
        if (playerPieces == null)
        {
            Debug.LogError("Player Pieces is null!!");
            return;
        }
        if (gameState == null)
        {
            Debug.LogError("GameState is null");
            return;
        }

        if (gameState.scores == null)
        {
            Debug.LogError("GameStates Scoreboard is null");
            return;
        }
        
        if (gameState.players == null)
        {
            Debug.LogError("GameStates Players is null");
            return;
        }
        
        Debug.Log(playerPieces.Count);
        if (playerPieces.Count <= 0)
        {
            return;
        }
        foreach (var player in gameState.players)
        {
            try
            {
                if (i > playerPieces.Count)
                {
                    Debug.Log("Creating new Player Piece");
                    Vector3 offset = new Vector3(0, 0.35f, 0);
                    Vector3 startPosition = GetStartPosition(i) + offset;
                    GameObject pieceInstance = Instantiate(playerPiecePrefabs[i], startPosition, Quaternion.identity);

                    // enabeling the mesh renderer
                    MeshRenderer meshRenderer = pieceInstance.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        meshRenderer.enabled = true;
                    }

                    playerPieces[i] = pieceInstance.GetComponent<PlayerPiece>();
                    playerPieces[i].SetPath(GetBoardPathForTeam(i));
                }

                
                
                if (!gameState.scores.ContainsKey(player.uuid))
                {
                    Debug.LogError(player.uuid + " not in Scoreboard");
                    continue;
                }
                playerPieces[i % totalPlayers].SetPosition(gameState.scores[player.uuid]);
                Debug.Log("Set Player " + i + "'s position to: " + gameState.scores[player.uuid]);
                ++i;
                i %= 4;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    public void AnswerQuestion(MultiplayerManager.QuestionData questionData, Action<string> callback)
    {
        try
        {
            StartCoroutine(AnswerQuestionCoroutine(questionData, callback));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    private IEnumerator AnswerQuestionCoroutine(MultiplayerManager.QuestionData questionData, Action<string> callback)
    {
        string answer = null;

        yield return StartCoroutine(WaitForAnswer(questionData, (result) => answer = result));

        callback(answer);
    }

    public void SetKillText(string message, bool kill)
    {
        StartCoroutine(ShowKillText(message, kill));
    }

    private IEnumerator ShowKillText(string message, bool kill)
    {
        AudioClip clip = null;
        
        if (!_killedAudio.IsUnityNull() && !_killAudio.IsUnityNull() && !_audioSource.IsUnityNull())
        {
            if (kill)
            {
                clip = _killAudio;
            }
            else
            {
                clip = _killedAudio;
            }
            _audioSource.clip = clip;
        }
        
        if (!_killText.IsUnityNull() && !_killCanvas.IsUnityNull())
        {
            _killText.text = message;
            _killCanvas.SetActive(true);
        }

        if (!clip.IsUnityNull())
        {
            _audioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }

        yield return new WaitForSeconds(2);

        _killCanvas.SetActive(false);
    }

    public void MovePlayerPiece(int playerindex, int steps)
    {
        if (playerindex >= 0 && playerindex < playerPieces.Count)
        {
            playerPieces[playerindex].MovePiece(steps);
        }
    }
    
    private IEnumerator WaitForAnswer(MultiplayerManager.QuestionData questionData, System.Action<string> callback)
    {
        Questions question = ConvertToQuestion(questionData);
        IEnumerator askQuestionCoroutine = questionsController.AskQuestion(question);
        yield return StartCoroutine(askQuestionCoroutine);

        string playerAnswer = askQuestionCoroutine.Current as string;
        callback(playerAnswer);
    }

    private IEnumerator AskQuestion(MultiplayerManager.QuestionData questionData)
    {
        IEnumerator askQuestionCoroutine;
        try
        {
            Questions question = ConvertToQuestion(questionData);
            askQuestionCoroutine = questionsController.AskQuestion(question);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        
        yield return StartCoroutine(askQuestionCoroutine);

        string playerAnswer = askQuestionCoroutine.Current as string;
    }

    private Questions ConvertToQuestion(MultiplayerManager.QuestionData questionData)
    {
        Questions q;
        try
        {
            string qText = questionData.question;
            string[] qAnswers = questionData.answers;
            string qTopic;
            if (questionData.topic.IsUnityNull())
            {
                qTopic = "";
            }
            else
            {
                qTopic = questionData.topic;
            }

            int points;

            if (questionData.points.IsUnityNull())
            {
                points = 0;
            }
            else
            {
                points = questionData.points;
            }
            
            q = new Questions(qText, qAnswers, " ", qTopic, points);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }

        return q;
    }
    
    // Called if a player is at the turn    
    public void AtTurn()
    {
        inventoryManager.UpdateInventoryUI(currentPlayerIndex);
        FieldEvent currEvent = inventoryManager.GetCurrentEvent(currentPlayerIndex);
        if (currEvent != null)
        {
            currEvent.SetStorable(false);
            DoFieldEvent(currentPlayerIndex, currEvent);
            inventoryManager.ClearCurrentEvent(currentPlayerIndex);
        }
        
        skipQuestion[currentPlayerIndex] = false;
    }

    public void EndGame(string winner)
    {
        //TODO implement winning message
    }

    // Called at the end of a turn
    public void EndTurn()
    {
        
    }

    /* Color parseHexColor(string hexColor){
        Color newColor;
        if(UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out newColor)){
            return newColor;
        } else {
            return Color.red;
        }
    }*/



    /* void ChangeSelectionColor(){
        switch(currentPlayerIndex){
            case 0:
                selectionImage.GetComponent<Image>().color = parseHexColor("#B6343B");
                selectionForeground.GetComponent<Image>().color = parseHexColor("#B6343B"); 
                selectionText.GetComponent<Image>().color = parseHexColor("#9B343C");
                selectionTitle.GetComponent<Image>().color = parseHexColor("#9B343C"); 
                break;
            case 1:
                selectionImage.GetComponent<Image>().color = parseHexColor("#245DD9");
                selectionForeground.GetComponent<Image>().color = parseHexColor("#245DD9"); 
                selectionText.GetComponent<Image>().color = parseHexColor("#1B39C6");
                selectionTitle.GetComponent<Image>().color = parseHexColor("#1B39C6"); 
                break;
            case 2:
                selectionImage.GetComponent<Image>().color = parseHexColor("#D9BC55");
                selectionForeground.GetComponent<Image>().color = parseHexColor("#D9BC55"); 
                selectionText.GetComponent<Image>().color = parseHexColor("#E7AB24");
                selectionTitle.GetComponent<Image>().color = parseHexColor("#E7AB24"); 
                break;
            case 3:
                selectionImage.GetComponent<Image>().color = parseHexColor("#628543");
                selectionForeground.GetComponent<Image>().color = parseHexColor("#628543"); 
                selectionText.GetComponent<Image>().color = parseHexColor("#455F41");
                selectionTitle.GetComponent<Image>().color = parseHexColor("#455F41"); 
                break;
        }
    }*/


    // Initialize all the player pieces and give them a position
    private void InitializePlayerPieces()
    {

        playerPieces = new List<PlayerPiece>();
        Vector3 offset = new Vector3(0, 0.35f, 0); // Move 1 unit higher on the y-axis

        for (int team = 0; team < totalPlayers; team++)
        {
                Vector3 startPosition = GetStartPosition(team) + offset;
                GameObject pieceInstance = Instantiate(playerPiecePrefabs[team], startPosition, Quaternion.identity);

                // enabeling the mesh renderer
                MeshRenderer meshRenderer = pieceInstance.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true;
                }

                playerPieces.Add(pieceInstance.GetComponent<PlayerPiece>());
                playerPieces[team].SetPath(GetBoardPathForTeam(team));
        }
        Debug.Log("Player Pieces initialized!");
    }

    private Vector3 GetStartPosition(int team)
    {
        // Define start positions based on team and piece index
        // For example, return positions from predefined start positions for each team
        switch (team)
        {
            case 0: return board.boardPathRed[0].transform.position;
            case 1: return board.boardPathBlue[0].transform.position;
            case 2: return board.boardPathYellow[0].transform.position;
            case 3: return board.boardPathGreen[0].transform.position;
            default: return Vector3.zero;
        }
    }

    private GameObject[] GetBoardPathForTeam(int team)
    {
        // Return the board path based on the team
        switch (team)
        {
            case 0: return board.boardPathRed;
            case 1: return board.boardPathBlue;
            case 2: return board.boardPathYellow;
            case 3: return board.boardPathGreen;
            default: return null;
        }
    }


    private bool CheckForWin()
    {
        // Check if one player has 4 players at home
        return false;
    }

    private void GetFieldEvent(int playerNumber)
    {
        FieldEvent fieldEvent = fieldEventController.GetFieldEvent();
        if (fieldEvent == null)
        {
            Debug.LogError("Field Event is null");
            return;
        }

        if(fieldEvent.IsStorable())
        {
            //ask Player if he wants to put it in Inventory
            //if player wants to put it in inventory -> set Storable to false and return;
            inventoryManager.AddCard(playerNumber, fieldEvent);
            return;
        }
        
        DoFieldEvent(playerNumber, fieldEvent);
    }

    private void DoFieldEvent(int playerNumber, FieldEvent fieldEvent)
    {
        switch (fieldEvent.GetEventType())
        {
            case "SkipQuestion":
                skipQuestion[playerNumber] = true;
                Debug.Log("Player " + (playerNumber + 1) + " can skip the next question");
                break;
        }
    }

    public bool GetSkipQuestion()
    {
        return skipQuestion[currentPlayerIndex];
    }
}
