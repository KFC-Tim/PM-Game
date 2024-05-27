using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{

    public GameObject[] path;
    public int currentPosition = 0;
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
            Debug.Log("Cannot move, target position is out of bounds.");
        }
    }
}
