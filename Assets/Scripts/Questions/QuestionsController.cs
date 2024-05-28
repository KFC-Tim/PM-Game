using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class QuestionsController : MonoBehaviour
{
    public GameMaster gameMaster;
    public GameObject questionCanvas;
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
        questions.Add(new Questions("Test2", new string[] {"1", "B", "3", "4"}, "B", 2, 4));
        questions.Add(new Questions("Test3", new string[] {"K", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test4", new string[] {"f", "B", "C", "A"}, "B", 2, 4));
        questions.Add(new Questions("Test5", new string[] {"R", "B", "M", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test6", new string[] {"A", "B", "C", "D"}, "B", 2, 4));
        questions.Add(new Questions("Test7", new string[] {"1", "B", "3", "4"}, "B", 2, 2));
        questions.Add(new Questions("Test8", new string[] {"K", "B", "C", "D"}, "B", 2, 4));
        questions.Add(new Questions("Test9", new string[] {"f", "B", "C", "A"}, "B", 2, 2));
        questions.Add(new Questions("Test10", new string[] {"R", "B", "M", "D"}, "B", 2, 4));
        questions.Add(new Questions("Test11", new string[] {"A", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test12", new string[] {"1", "B", "3", "4"}, "B", 2, 4));
        questions.Add(new Questions("Test13", new string[] {"K", "B", "C", "D"}, "B", 2, 2));
        questions.Add(new Questions("Test14", new string[] {"f", "B", "C", "A"}, "B", 2, 4));
        questions.Add(new Questions("Test15", new string[] {"R", "B", "M", "D"}, "B", 2, 2));

        answerButton1.onClick.AddListener(() => EvaluateAnswer(answerButton1));
        answerButton2.onClick.AddListener(() => EvaluateAnswer(answerButton2));
        answerButton3.onClick.AddListener(() => EvaluateAnswer(answerButton3));
        answerButton4.onClick.AddListener(() => EvaluateAnswer(answerButton4));
    }

    public void AskQuestion()
    {
        currentQuestion = questions[Random.Range(0, questions.Count)];
        questionText.text = currentQuestion.questionText; 
        UpdateAnswerButtons(currentQuestion.questionAnsers);
        ShowCanvas();

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

        StartCoroutine(WaitAndEndTurn(isCorrect));  
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

    private IEnumerator WaitAndEndTurn(bool isCorrect)
    {
        // Waiting to see the result
        yield return new WaitForSeconds(2);


        // Hide the canvas
        ResetButtonColors();
        HideCanvas();

        if (isCorrect)
        {

            Vector3 targetFieldPosition = gameMaster.playerPieces[currentPlayerIndex].path[gameMaster.playerPieces[currentPlayerIndex].currentPosition + currentQuestion.steps].transform.position;
            // Move the player piece here while the board is displayed
            gameMaster.playerPieces[gameMaster.currentPlayerIndex].MovePiece(currentQuestion.steps);

            if (!gameMaster.IsPlayerOnField(targetFieldPosition))
            {
                gameMaster.playerPieces[currentPlayerIndex, currentPlayerPieceIndex].MovePiece(currentQuestion.steps);
            }
            else
            {
                Debug.Log("Cannot move, another player is on the target field.");
            }
        }

        yield return new WaitForSeconds(5);

        
        // Waiting to see the board

        // After UI updated en the turn
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

    private void ShowCanvas(){
        questionCanvas.SetActive(true);
    }

    private void HideCanvas(){
        questionCanvas.SetActive(false);
    }

   
}
