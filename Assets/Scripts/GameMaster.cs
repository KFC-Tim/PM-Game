using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public InventoryManager inventoryManager;
    [SerializeField] private QuestionsController questionsController;
    private FieldEventController fieldEventController;

    [SerializeField] private GameObject[] playerPiecePrefabs = new GameObject[4];
    public PlayerPiece[,] playerPieces;

    [SerializeField] public Button pieceButton1;
    [SerializeField] public Button pieceButton2;
    [SerializeField] public Button pieceButton3;
    [SerializeField] public Button pieceButton4;

    [SerializeField] private Board board;

    [SerializeField] private GameObject selectionCanvas;
    [SerializeField] private GameObject selectionImage;
    [SerializeField] private GameObject selectionForeground;
    [SerializeField] private GameObject selectionTitle;
    [SerializeField] private GameObject selectionText;

    [SerializeField] private int totalPlayers = 4; // required to have 4 players
    [SerializeField] private int piecesPerPlayer = 4; // required to have 4 pieces

    public int currentPlayerIndex = 0;
    private int currentPlayerPieceIndex = 0;

    private bool gameIsOver = false;
    private bool[] skipQuestion = {false, false, false, false};
    private bool hasSelected = false;



    // Start is called before the first frame update
    void Start()
    {
        if(board == null){
            Debug.Log("Board component not foun in the scene");
            return;
        }

        InitializePlayerPieces();

        // maybe here the random or by join the lobby
        currentPlayerIndex = 0;
        gameIsOver = false;

        // Starts the game
        AtTurn();
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
        
        
        if (gameIsOver)
        {
            // TODO change scene
            Debug.Log("Game is over. No more turns.");
            return;
        }
        if (!skipQuestion[currentPlayerIndex])
        {
            StartCoroutine(ShowPlayerPiceSelection());
        }
        skipQuestion[currentPlayerIndex] = false;

        // TODO move by qutetions steps
        playerPieces[currentPlayerIndex, currentPlayerIndex].MovePiece(2);
    }

    // Called at the end of a turn
    public void EndTurn()
    {
        if (CheckForWin())
        {
            Debug.Log("Player " +  (currentPlayerIndex + 1)  +  " wins!");
            gameIsOver = true;
            // Return to WinningSequence;
            return;
        }
        if (!gameIsOver)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
            Debug.Log("Player " + (currentPlayerIndex + 1) + "'s turn.");
            AtTurn();
        }
        if (gameIsOver)
        {      
            Debug.Log("Game is over. No more turns.");
            // Return to EndSequence
            return;
        }
        

    }

    private Color parseHexColor(string hexColor){
        Color newColor;
        if(UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out newColor)){
            return newColor;
        } else {
            return Color.red;
        }
    }

    public IEnumerator ShowPlayerPiceSelection(){

        ChangeSelectionColor();

        selectionCanvas.SetActive(true);
        
        hasSelected = false;

        pieceButton1.onClick.AddListener(() => SelectPlayerPiece(0));
        pieceButton2.onClick.AddListener(() => SelectPlayerPiece(1));
        pieceButton3.onClick.AddListener(() => SelectPlayerPiece(2));
        pieceButton4.onClick.AddListener(() => SelectPlayerPiece(3));

        // Wait until player got selected
        yield return new WaitUntil(() => hasSelected);

        pieceButton1.onClick.RemoveAllListeners();
        pieceButton2.onClick.RemoveAllListeners();
        pieceButton3.onClick.RemoveAllListeners();
        pieceButton4.onClick.RemoveAllListeners();

        questionsController.AskQuestion();
   
    }

    // Player can select one player piece to move forword with
    private void SelectPlayerPiece(int pieceNumber)
    {
        currentPlayerPieceIndex = pieceNumber;
        hasSelected = true;
        Debug.Log("Player " + currentPlayerIndex + " got selected!");

        selectionCanvas.SetActive(false);
    }

    private void ChangeSelectionColor(){
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
    }


    // Initialize all the player pieces and give them a position
    private void InitializePlayerPieces()
    {

        playerPieces = new PlayerPiece[totalPlayers, piecesPerPlayer];
        Vector3 offset = new Vector3(0, 0.35f, 0); // Move 1 unit higher on the y-axis

        for (int team = 0; team < totalPlayers; team++)
        {
            for (int pieceIndex = 0; pieceIndex < piecesPerPlayer; pieceIndex++)
            {
                Vector3 startPosition = GetStartPosition(team, pieceIndex) + offset;
                GameObject pieceInstance = Instantiate(playerPiecePrefabs[team], startPosition, Quaternion.identity);

                // enabeling the mesh renderer
                MeshRenderer meshRenderer = pieceInstance.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true;
                }

                playerPieces[team, pieceIndex] = pieceInstance.GetComponent<PlayerPiece>();
                playerPieces[team, pieceIndex].SetPath(GetBoardPathForTeam(team));
            }
        }
    }

    private Vector3 GetStartPosition(int team, int pieceIndex)
    {
        // Define start positions based on team and piece index
        // For example, return positions from predefined start positions for each team
        switch (team)
        {
            case 0: return board.boardStartingRed[pieceIndex].transform.position;
            case 1: return board.boardStartingBlue[pieceIndex].transform.position;
            case 2: return board.boardStartingYellow[pieceIndex].transform.position;
            case 3: return board.boardStartingGreen[pieceIndex].transform.position;
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
