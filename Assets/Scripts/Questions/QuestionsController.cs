using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.LowLevel;

public class QuestionsController : MonoBehaviour
{
    public GameObject questionCanvas;
    private List<Questions> questions = new List<Questions>();
    public TMP_Text questionText;
    public TMP_Text pointsText;
    public TMP_Text topicText;
    
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;
    public TMP_Text answerButtonText1;
    public TMP_Text answerButtonText2;
    public TMP_Text answerButtonText3;
    public TMP_Text answerButtonText4;

    private TMP_Text[] _answerButtonTexts;
    private Button[] _answerButtons;
    
    private Questions currentQuestion;
    private string playerAnswer;
    
    private IGameController gameController;
    
    public static QuestionsController Instance { get; private set; }

    [System.Serializable]
    public class AnswerEvent : UnityEvent<string> { };

    public AnswerEvent OnAnswerSubmitted;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("QuestionsController started!");

        answerButton1.onClick.AddListener(() => OnAnswerButtonClick(answerButton1));
        answerButton2.onClick.AddListener(() => OnAnswerButtonClick(answerButton2));
        answerButton3.onClick.AddListener(() => OnAnswerButtonClick(answerButton3));
        answerButton4.onClick.AddListener(() => OnAnswerButtonClick(answerButton4));

        _answerButtonTexts = new[] { answerButtonText1, answerButtonText2, answerButtonText3, answerButtonText4 };
        _answerButtons = new[] { answerButton1, answerButton2, answerButton3, answerButton4 };

    }
    
    void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    public IEnumerator AskQuestion(Questions q)
    {
        currentQuestion = q;
        questionText.text = q.questionText;
        topicText.text = ToTopic(q.topic);
        pointsText.text = q.steps.ToString();
        UpdateAnswerButtons(q.questionAnsers);
        ShowCanvas();

        playerAnswer = null;
        OnAnswerSubmitted.AddListener((answer) => playerAnswer = answer);

        while (string.IsNullOrEmpty(playerAnswer))
        {
            yield return null;
        }
        
        OnAnswerSubmitted.RemoveAllListeners();
        HideCanvas();

        yield return playerAnswer;
    }
    
    public void SetGameController(IGameController controller)
    {
        gameController = controller;
    }

    private string ToTopic(string topic)
    {
        if (topic.Equals("MKT"))
        {
            return "Marketing";
        }

        return topic;
    }

    private void OnAnswerButtonClick(Button clickedButton)
    {
        string answer = clickedButton.GetComponentInChildren<TMP_Text>().text;
        OnAnswerSubmitted.Invoke(answer);
        //EvaluateAnswer(answer, clickedButton);
    }
    
    private void EvaluateAnswer(string answer, Button clickedButton)
    {
        bool isCorrect = answer.ToLower() == currentQuestion.correctAnswer.ToLower();

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
            // Move the player piece here while the board is displayed
            //gameMaster.playerPieces[gameMaster.currentPlayerIndex].MovePiece(currentQuestion.steps);
            gameController.MovePlayerPiece(-1, currentQuestion.steps);

        }

        yield return new WaitForSeconds(5);

        
        // Waiting to see the board
        

        // After UI updated en the turn
        //gameMaster.EndTurn();
        gameController.EndTurn();
    }

    private void UpdateAnswerButtons(string[] answers)
    {
        if (answers.IsUnityNull())
        {
            Debug.LogError("Antworten-Array ist null");
        }

        for (int i = 0; i < _answerButtons.Length; ++i)
        {
            _answerButtonTexts[i].text = "";
            _answerButtons[i].enabled = false;
            _answerButtons[i].interactable = false;
        }
        
        for (int i = 0; i <  math.min(answers.Length, _answerButtons.Length); ++i)
        {
            _answerButtonTexts[i].text = answers[i];
            _answerButtons[i].interactable = true;
            _answerButtons[i].enabled = true;
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
