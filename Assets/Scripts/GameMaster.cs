using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameMaster : MonoBehaviour
{
    public QuestionsController QuestionsController;

    private int currentPlayerIndex = 0;
    private int totalPlayers = 4; // required to have 4 players
    private bool gameIsOver = false;


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
        Debug.Log("Player " + (currentPlayerIndex + 1) + " answer this qustions:");
        Debug.Log(question);

        // Here comes the UserInput
        string playerAnswer = "";

        QuestionsController.EvaluateAnswer(playerAnswer, question); 

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

}
