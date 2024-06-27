using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
using TMPro;

public class ConnectButton : MonoBehaviour
{
    public Button connectButton;
    public Image ErrorMessageSpielername;
    public Image ErrorMessageSessionID;
    public Image Error;
    public TMP_InputField nameInput;
    public TMP_InputField sessionInput;
    public MultiplayerManager multiplayerManager; 

    void Start()
    {
        

        ErrorMessageSpielername.gameObject.SetActive(false);
        ErrorMessageSessionID.gameObject.SetActive(false);
        Error.gameObject.SetActive(false);

        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectClicked);
        }

        CrossSceneInformation.MultiplayerManager = multiplayerManager;
    }

    

    private void OnConnectClicked()
    {
        bool nameMissing = string.IsNullOrEmpty(nameInput.text);
        bool sessionMissing = string.IsNullOrEmpty(sessionInput.text);

        if (!nameMissing && !sessionMissing)
        {
            Debug.Log("Name: " + nameInput.text + ", Session: " + sessionInput.text);
            multiplayerManager.JoinGame(sessionInput.text, nameInput.text);
        }
        else
        {
            // Eingaben fehlen, entsprechende Fehlermeldungen anzeigen
            if (nameMissing && sessionMissing)
            {
                StartCoroutine(ShowErrorMessage(Error));
            }
            else if (nameMissing)
            {
                StartCoroutine(ShowErrorMessage(ErrorMessageSpielername));
            }
            else if (sessionMissing)
            {
                StartCoroutine(ShowErrorMessage(ErrorMessageSessionID));
            }
        }
    }

    private IEnumerator ShowErrorMessage(Image errorMessageImage)
    {
        errorMessageImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f); // Warten für 3 Sekunden
        errorMessageImage.gameObject.SetActive(false);
    }
}
