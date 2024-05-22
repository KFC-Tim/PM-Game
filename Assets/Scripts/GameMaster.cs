using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public QuestionsController QuestionsController;
    public TMP_InputField inputField;
    public Button submitButton;

    public PlayerPiece[] playerPiecePrefab;

    private PlayerPiece[,] playerPieces;
    private Questions currentQuestion;
    private int currentPlayerIndex = 0;
    private int totalPlayers = 4; // required to have 4 players
    private int piecesForPlayer = 4;
    private bool gameIsOver = false;


    // Start is called before the first frame update
    void Start()
    {
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
        currentQuestion = QuestionsController.AskQuestion();
        Debug.Log("Player " + (currentPlayerIndex + 1) + " answer this qustions:");
        Debug.Log(currentQuestion.questionText);

        inputField.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);

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
        playerPieces = new PlayerPiece[totalPlayers, piecesForPlayer];

        for(int i = 0; i < totalPlayers; i++)
        {
            for(int j = 0;  j < piecesForPlayer; j++)
            {
                PlayerPiece newPiece = Instantiate(playerPiecePrefab[i], CalculateStartPosition(i, j), Quaternion.identity);
                newPiece.name = "Player " + (i + 1) + " Piece " + (j + 1);
                newPiece.transform.parent = transform;
                playerPieces[i, j] = newPiece;
            }
        }
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

}
