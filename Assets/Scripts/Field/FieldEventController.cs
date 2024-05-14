using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEventController : MonoBehaviour
{
    private List<FieldEvent> _fields = new List<FieldEvent>();
    private int _currentCard;

    // Start is called before the first frame update
    void Start()
    {
        _fields.Add(new SkipQuestionFieldEvent());
        _fields.Add(new SkipQuestionFieldEvent());
        _fields.Add(new SkipQuestionFieldEvent());

        //TODO implement shuffle cards
        _currentCard = _fields.Count - 1;
    }

    public FieldEvent GetFieldEvent()
    {
        if(_currentCard == -1)
        {
            //TODO implement shuffle cards
            _currentCard = _fields.Count - 1;
        }
        return _fields[_currentCard--];
    }
    
}
