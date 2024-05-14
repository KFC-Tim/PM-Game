using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkipQuestionFieldEvent : FieldEvent
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void DoEvent()
    {
        //TODO implement posibility to skip question ^^
        Debug.Log("Question skipped");
    }
}
