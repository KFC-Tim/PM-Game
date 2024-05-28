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
    public FieldEventController fieldEventController;

    [SerializeField] private GameObject[] playerPiecePrefabs = new GameObject[4];
    public PlayerPiece[] playerPieces;
    public int[] playerRounds = new int[4];


    [SerializeField] private Board board;

    [SerializeField] private int totalPlayers = 4; // required to have 4 players

    public int currentPlayerIndex = 0;

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
        //TODO implement EndTurn when SkipQuestion is true
        if (!skipQuestion[currentPlayerIndex])
        {
            questionsController.AskQuestion();
        }
        else
        {
            EndTurn();
        }
        skipQuestion[currentPlayerIndex] = false;

        // TODO move by qutetions steps
        GameObject currPositionGameObj = playerPieces[currentPlayerIndex].GetGameObjectPosition();
        Field currField = currPositionGameObj.GetComponent<Field>();

        if (!currField.IsUnityNull() && currField.IsEventField)
        {
            GetFieldEvent(currentPlayerIndex);
        }
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

        playerPieces = new PlayerPiece[totalPlayers];
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

                playerPieces[team] = pieceInstance.GetComponent<PlayerPiece>();
                playerPieces[team].SetPath(GetBoardPathForTeam(team));
            
        }
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
