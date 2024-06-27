using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public bool IsEventField = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.name + " loaded!");
    }
}
