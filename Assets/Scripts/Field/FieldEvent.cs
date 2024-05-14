using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FieldEvent : MonoBehaviour
{
    private bool storable;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public FieldEvent(bool storable = false)
    {
        this.storable = storable;
    }

    public abstract void DoEvent();
}
