using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{

    public GameObject[] path;
    private int currentPosition = 0;
    private int currentRounds = 0;
    public bool isHome;

    // Method to set the path for this player piece
    public void SetPath(GameObject[] newPath)
    {
        path = newPath;
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
