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
    public TMP_InputField inputField;
    public Button submitButton;

    public GameObject[] playerPiecePrefab = new GameObject[4];
    private PlayerPiece[] playerPieces = new PlayerPiece[4];

    //private PlayerPiece[,] playerPieces;
    private Questions currentQuestion;
    private int currentPlayerIndex = 0;
    private int totalPlayers = 4; // required to have 4 players
    private int piecesForPlayer = 4;
    private bool gameIsOver = false;
    private bool skipQuestion = false;

    private Board board;


    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();

        InitializePlayerPieces();

        submitButton.onClick.AddListener(SubmitAnswer);

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
    public void AtTurn()
    {
        // Get a question
        Questions question = QuestionsController.AskQuestion();

        if (!skipQuestion)
        {
            currentQuestion = QuestionsController.AskQuestion();
            Debug.Log("Player " + (currentPlayerIndex + 1) + " answer this qustions:");
            Debug.Log(currentQuestion.questionText);

            inputField.gameObject.SetActive(true);
            submitButton.gameObject.SetActive(true);
        }


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
        if (gameIsOver)
        {      
            Debug.Log("Game is over. No more turns.");
            // Return to EndSequence
            return;
        }
        if (CheckForWin())
        {
            Debug.Log("Player " +  (currentPlayerIndex + 1)  +  " wins!");
            gameIsOver = true;
            // Return to WinningSequence;
            return;
        }

        // Move player on board
        playerPieces[currentPlayerIndex].MovePiece(2);

        // Move to the next player 
        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
        // Show in an alert or somewhere
        Debug.Log("Player " + (currentPlayerIndex + 1) +"'s turn.");


        AtTurn();

    }

    // Player can select one player piece to move forword with
    private void SelectPlayerToMove()
    {

    }

    // Player moves forword with his piece
    private void MovePlayerPiece(int playerIndex, int steps)
    {

    }

    // Initialize all the player pieces and give them a position
    private void InitializePlayerPieces()
    {
        Vector3 offset = new Vector3(0, 0.35f, 0); // Move 1 unit higher on the y-axis

        playerPieces[0] = Instantiate(playerPiecePrefab[0], board.boardPathRed[0].transform.position + offset, Quaternion.identity).GetComponent<PlayerPiece>();
        playerPieces[0].SetPath(board.boardPathRed);

        playerPieces[1] = Instantiate(playerPiecePrefab[1], board.boardPathBlue[0].transform.position + offset, Quaternion.identity).GetComponent<PlayerPiece>();
        playerPieces[1].SetPath(board.boardPathBlue);

        playerPieces[2] = Instantiate(playerPiecePrefab[2], board.boardPathYellow[0].transform.position + offset, Quaternion.identity).GetComponent<PlayerPiece>();
        playerPieces[2].SetPath(board.boardPathYellow);

        playerPieces[3] = Instantiate(playerPiecePrefab[3], board.boardPathGreen[0].transform.position + offset, Quaternion.identity).GetComponent<PlayerPiece>();
        playerPieces[3].SetPath(board.boardPathGreen);
    }

    // Submit the answer an evalutes the answer
    private void SubmitAnswer()
    {
        string playerAnswer = inputField.text;
        QuestionsController.EvaluateAnswer(playerAnswer, currentQuestion);

        inputField.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);

        inputField.text = "";

        EndTurn();
    }

    // Calcualtes the start position of the player pieces
    private Vector3 CalculateStartPosition(int playerIndex, int pieceIndex)
    {
        // Here comes the calcualtion
        return new Vector3(playerIndex, 0, pieceIndex);
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
