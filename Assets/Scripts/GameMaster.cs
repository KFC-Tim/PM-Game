using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GameMaster : MonoBehaviour
{
    public QuestionsController QuestionsController;
    private FieldEventController fieldEventController;

    private int currentPlayerIndex = 0;
    private int totalPlayers = 4; // required to have 4 players
    private bool gameIsOver = false;
    private bool skipQuestion = false;


    // Start is called before the first frame update
    void Start()
    {
        // maybe here the random or by join the lobby
        currentPlayerIndex = 0;
        gameIsOver = false;
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
            Debug.Log("Player " + (currentPlayerIndex + 1) + " answer this qustions:");
            Debug.Log(question);

            // Here comes the UserInput
            string playerAnswer = "";

            QuestionsController.EvaluateAnswer(playerAnswer, question);
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

        // Move to the next player 
        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
        // Show in an alert or somewhere
        Debug.Log("Player " + (currentPlayerIndex + 1) +"'s turn.");

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
