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
    public QuestionsController QuestionsController;
    private FieldEventController fieldEventController;

    public GameObject[] playerPiecePrefabs = new GameObject[4];
    private PlayerPiece[,] playerPieces;

    private int currentPlayerIndex = 0;
    private int currentPlayerPieceIndex = 0;
    private int totalPlayers = 4; // required to have 4 players
    private int piecesPerPlayer = 4; // required to have 4 pieces
    private bool gameIsOver = false;
    private bool skipQuestion = false;
    private bool hasSelected = false;
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;

    public Button pieceButton1;
    public Button pieceButton2;
    public Button pieceButton3;
    public Button pieceButton4;

    private Board board;

    public GameObject selcetionCanvas;


    // Start is called before the first frame update
    void Start()
    {

        board = FindObjectOfType<Board>();
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


    // Update is called once per frame
    void Update()
    {
        
    }

    // Called if a player is at the turn 
    public void PreTurn(){
        // In this Fucntion i want to selcet the playerpiece aand start at turn
        if (gameIsOver)
        {
            Debug.Log("Game is over. No more turns.");
            return;
        }



        AtTurn();
    }


    // Called if a player is at the turn    
    public void AtTurn()
    {
        Debug.Log("At Turn");
        if (gameIsOver)
        {
            Debug.Log("Game is over. No more turns.");
            return;
        }
        if (!skipQuestion)
        {
            StartCoroutine(ShowPlayerPiceSelection());
            hasSelected = false;
        
        }


        playerPieces[currentPlayerIndex, currentPlayerIndex].MovePiece(2);

        //if question was right or skipped
        //move x fields with selected player
        Field playerField = new Field();
        if(playerField.IsEventField)
        {
            DoFieldEvent();
        }

        skipQuestion = false;
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

    public IEnumerator ShowPlayerPiceSelection(){

        selcetionCanvas.SetActive(true);
        Debug.Log("Show Canvas");

        pieceButton1.onClick.AddListener(() => SelectPlayerPiece(1));
        pieceButton2.onClick.AddListener(() => SelectPlayerPiece(2));
        pieceButton3.onClick.AddListener(() => SelectPlayerPiece(3));
        pieceButton4.onClick.AddListener(() => SelectPlayerPiece(4));

        // Wait until player got selected
        yield return new WaitUntil(() => hasSelected);

        QuestionsController.AskQuestion();
   
    }

    // Player can select one player piece to move forword with
    private void SelectPlayerPiece(int pieceNumber)
    {
        currentPlayerPieceIndex = pieceNumber;
        hasSelected = true;
        Debug.Log("Player " + currentPlayerIndex + " got selected!");

        selcetionCanvas.SetActive(false);
    }

    // Player moves forword with his piece
    private void MovePlayerPiece(int playerIndex, int steps)
    {

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

    private void DoFieldEvent()
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
        }

        switch(fieldEvent.GetEventType())
        {
            case "SkipQuestion":
                skipQuestion = true;
                break;
        }
        
    }

}
