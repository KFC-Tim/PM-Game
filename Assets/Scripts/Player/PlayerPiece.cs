using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{
    public Vector3 startPosition;
    public bool isHome;

    public PlayerPiece(Vector3 pos)
    {
        startPosition = pos;
        isHome = true;

    }
}
