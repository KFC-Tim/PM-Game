using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldEventController : MonoBehaviour
{
    private List<FieldEvent> _fields = new List<FieldEvent>();
    private int _currentCard;

    // Start is called before the first frame update
    void Start()
    {
        _fields.Add(new FieldEvent());
        _fields.Add(new FieldEvent());
        _fields.Add(new FieldEvent());

        Shuffle();
        _currentCard = _fields.Count - 1;
    }

    //returns the next FieldEventCard on a "stack" of card´s,
    //if "stack" is empty shuffle and start at the top of the "stack"
    public FieldEvent GetFieldEvent()
    {
        if(_currentCard == -1)
        {
            Shuffle();
            _currentCard = _fields.Count - 1;
        }
        return _fields[_currentCard--];
    }
    
    //Shuffles the FieldEvents List with Random
    private void Shuffle()
    {
        System.Random _rand = new System.Random();
        _fields = _fields.OrderBy(x => _rand.Next()).ToList();
    }
}
