using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private const int maxGameFields = 44;
    public GameObject[] boardPathRed = new GameObject[maxGameFields];
    public GameObject[] boardPathBlue = new GameObject[maxGameFields];
    public GameObject[] boardPathYellow = new GameObject[maxGameFields];
    public GameObject[] boardPathGreen = new GameObject[maxGameFields];
}