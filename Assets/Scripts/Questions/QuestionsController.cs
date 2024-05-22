using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QuestionsController : MonoBehaviour
{
    private List<Questions> questions = new List<Questions>();


    // Start is called before the first frame update
    void Start()
    {
        questions.Add(new Questions("Test1", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test2", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test3", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test4", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test5", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
    }


    public Questions AskQuestion()
    {
        Questions question = questions[Random.Range(0, questions.Count)];
        return question;

    }

    public void EvaluateAnswer(string playerAnswer, Questions question)
    {
        if (playerAnswer.ToLower() == question.correctAnswer.ToLower())
        {
            Debug.Log("Correct! Player can move " + question.steps + " fields forword!");
            RemoveQuestion(question);
        }
        else
        {
            Debug.Log("Incorrect! Player can't move !");
        }

        return;
    }

    private void RemoveQuestion(Questions question)
    {
        questions.Remove(question);
    }

   
}
