using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class CreateButton : MonoBehaviour
{
    public Button myButton;
    public TMP_InputField nameInput;
    public MultiplayerManager multiplayerManager;

    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClick);
        }
        CrossSceneInformation.MultiplayerManager = multiplayerManager;
    }

    // Diese Methode wird aufgerufen, wenn der Button geklickt wird
    private void OnButtonClick()
    {
        Debug.Log("Create Session");
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            multiplayerManager.CreateGame(nameInput.text); 
        }
        else
        {
            Debug.LogWarning("Player name is missing");
        }
    }
}
