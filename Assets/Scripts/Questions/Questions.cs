using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Questions
{
    public string questionText;
    public string[] questionAnsers = new string[4];
    public string correctAnswer;
    public int difficulty;
    public int steps;

    public Questions(string questionText, string[] questionAnsers, string correctAnswer, int difficulty, int steps)
    {
        this.questionText = questionText;
        this.questionAnsers = questionAnsers;
        this.correctAnswer = correctAnswer;
        this.difficulty = difficulty;
        this.steps = steps;
    }

}
