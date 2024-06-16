using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionInput : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button submitButton;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.name + " loaded!");
        submitButton.onClick.AddListener(SubmitAnswer);
    }

    void SubmitAnswer()
    {
        string answer = inputField.text;

        // Here comes a connection to the master 

        inputField.text = "";
    }

    public void ButtonDemo()
    {
        Debug.Log(inputField.text);
    }
}
