using UnityEngine;
using UnityEngine.UI; // Wichtig für die UI-Komponenten

public class CreateButton : MonoBehaviour
{
    public Button myButton;

    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClick);
        }
    }

    // Diese Methode wird aufgerufen, wenn der Button geklickt wird
    private void OnButtonClick()
    {
        Debug.Log("Create Session");
    }
}
