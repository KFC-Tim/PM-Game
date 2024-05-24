using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class QuestionsController : MonoBehaviour
{
    public GameMaster gameMaster;
    private List<Questions> questions = new List<Questions>();
    public TMP_Text questionText;
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;
    public TMP_Text answerButtonText1;
    public TMP_Text answerButtonText2;
    public TMP_Text answerButtonText3;
    public TMP_Text answerButtonText4;

    private Questions currentQuestion;


    // Start is called before the first frame update
    void Start()
    {
        questions.Add(new Questions("Test1", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test2", new string[] {"1", "2", "3", "4"}, "2", 2, 2));
        questions.Add(new Questions("Test3", new string[] {"K", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test4", new string[] {"f", "B", "C", "A"}, "A", 2, 2));
        questions.Add(new Questions("Test5", new string[] {"R", "B", "M", "D"}, "M", 2, 2));
        DisplayQuestion();
        answerButton1.onClick.AddListener(() => EvaluateAnswer(answerButton1));
        answerButton2.onClick.AddListener(() => EvaluateAnswer(answerButton2));
        answerButton3.onClick.AddListener(() => EvaluateAnswer(answerButton3));
        answerButton4.onClick.AddListener(() => EvaluateAnswer(answerButton4));
    }

    public void DisplayQuestion()
    {
        Questions question = AskQuestion();
        questionText.text = question.questionText; 
        UpdateAnswerButtons(question.questionAnsers);
    
    }

    public Questions AskQuestion()
    {
        currentQuestion = questions[Random.Range(0, questions.Count)];
        return currentQuestion;

    }

    public void EvaluateAnswer(Button clickedButton)
     {
        string playerAnswer = clickedButton.GetComponentInChildren<TMP_Text>().text;
        bool isCorrect = playerAnswer.ToLower() == currentQuestion.correctAnswer.ToLower();

        if (isCorrect)
        {
            Debug.Log("Correct! Player can move " + currentQuestion.steps + " fields forward!");
            clickedButton.GetComponent<Image>().color = Color.green;
            RemoveQuestion(currentQuestion);
        }
        else
        {
            Debug.Log("Incorrect! Player can't move!");
            clickedButton.GetComponent<Image>().color = Color.red;
            HighlightCorrectAnswer();
        }

        StartCoroutine(WaitAndEndTurn());  
    }

    private void HighlightCorrectAnswer()
    {
        Button[] answerButtons = {answerButton1, answerButton2, answerButton3, answerButton4};
        foreach (Button answerButton in answerButtons)
        {
            TMP_Text buttonText = answerButton.GetComponentInChildren<TMP_Text>();
            if (buttonText.text.ToLower() == currentQuestion.correctAnswer.ToLower())
            {
                answerButton.GetComponent<Image>().color = Color.green;
                break; 
            }
        }
    }

    private IEnumerator WaitAndEndTurn()
    {
        //waiting for colors
        yield return new WaitForSeconds(1);
        ResetButtonColors();
        gameMaster.EndTurn();
    }

    private void UpdateAnswerButtons(string[] answers)
    {
         if (answers != null && answers.Length >= 4)
        {
            answerButtonText1.text = answers[0];
            answerButtonText2.text = answers[1];
            answerButtonText3.text = answers[2];
            answerButtonText4.text = answers[3];
        }
        else
        {
            Debug.LogError("Antworten-Array hat nicht die erwartete Länge von 4.");
        }
    }

    private void ResetButtonColors()
    {
        answerButton1.GetComponent<Image>().color = Color.white;
        answerButton2.GetComponent<Image>().color = Color.white;
        answerButton3.GetComponent<Image>().color = Color.white;
        answerButton4.GetComponent<Image>().color = Color.white;
    }

    private void RemoveQuestion(Questions question)
    {
        questions.Remove(question);
    }

   
}
