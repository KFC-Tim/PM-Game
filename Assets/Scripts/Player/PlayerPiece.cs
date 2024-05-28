using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{

    public GameObject[] path;
    public int currentPosition = 0;
    private int currentRounds = 0;
    private bool isHome;

    // Method to set the path for this player piece
    public void SetPath(GameObject[] newPath)
    {
        path = newPath;
    }

    // Method increases the currentRounds by one
    public void SetCurrentRounds(){
        currentRounds++;
    }

    // Method returns the current Rounds
    public int GetCurrentRounds(){
        return currentRounds;
    }

    // Method sets the current position
    public void SetCurrentPosition(int currentPosition){
        this.currentPosition = currentPosition;
    }

    // Method returns the current position
    public int GetCurrentPosition(){
        return currentPosition;
    }

    // Method to move the player piece along the path
    public void MovePiece(int steps)
    {
        int targetPosition = currentPosition + steps;

        if (targetPosition < path.Length) 
        {
            transform.position = path[targetPosition].transform.position;
            currentPosition = targetPosition;
        }
        else
        {
            currentRounds++;
            transform.position = path[targetPosition%40].transform.position;
            currentPosition = targetPosition%40;
        }
    }

    public GameObject GetGameObjectPosition()
    {
        return path[currentPosition];
    }
}
