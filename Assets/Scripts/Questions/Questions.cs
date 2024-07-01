using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Questions
{
    public string questionText;
    public string[] questionAnsers = new string[4];
    public string correctAnswer;
    public string topic;
    public int steps;

    public Questions(string questionText, string[] questionAnsers, string correctAnswer, string topic, int steps)
    {
        this.questionText = questionText;
        this.questionAnsers = questionAnsers;
        this.correctAnswer = correctAnswer;
        this.topic = topic;
        this.steps = steps;
    }

}
