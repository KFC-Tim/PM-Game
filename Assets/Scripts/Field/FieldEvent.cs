using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEvent : MonoBehaviour
{
    private bool storable;
    private string eventType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public FieldEvent(string eventType = "SkipQuestion", bool storable = false)
    {
        this.eventType = eventType;
        this.storable = storable;
    }

    public bool IsStorable() => storable;
    public void SetStorable(bool storable) { this.storable = storable; }

    public string GetEventType() => eventType;
    public void SetEventType(string eventType) {  this.eventType = eventType; }

}
