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
    public FieldEventController fieldEventController;
    public InventoryManager inventoryManager;

    public PlayerPiece[] playerPiecePrefab;

    private PlayerPiece[,] playerPieces;
    private Questions currentQuestion;
    private int currentPlayerIndex = 0;
    private int totalPlayers = 4; // required to have 4 players
    private int piecesForPlayer = 4;
    private bool gameIsOver = false;
    private bool[] skipQuestion = {false, false, false, false};
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;
    
    // Start is called before the first frame update
    void Start()
    {
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
    public void AtTurn()
    {
        inventoryManager.UpdateInventoryUI(currentPlayerIndex);
        (FieldEvent, int) currEvent = inventoryManager.GetCurrentEvent();
        if (currEvent.Item2 == currentPlayerIndex)
        {
            currEvent.Item1.SetStorable(false);
            DoFieldEvent(currentPlayerIndex, currEvent.Item1);
        }
        
        if (gameIsOver)
        {
            Debug.Log("Game is over. No more turns.");
            return;
        }
        if (!skipQuestion[currentPlayerIndex])
        {
            //currentQuestion = QuestionsController.AskQuestion();
            //Debug.Log("Player " + (currentPlayerIndex + 1) + " answer this qustions:");
            //Debug.Log(currentQuestion.questionText);
            QuestionsController.DisplayQuestion();
        }
        //if question was right or skipped
        //move x fields with selected player
        //Field playerField = new Field();
        //if(playerField.IsEventField)
        //{
        //    GetFieldEvent(currentPlayerIndex);
        //}

        skipQuestion[currentPlayerIndex] = false;
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
        switch(fieldEvent.GetEventType())
        {
            case "SkipQuestion":
                skipQuestion[playerNumber] = true;
                Debug.Log("Player " + (playerNumber+1) + " can skip the next question");
                break;
        }
    }

}
