using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject textPrefab;
    
    private List<FieldEvent>[] eventsInventory = new List<FieldEvent>[4];

    // Start is called before the first frame update
    void Start()
    {
        FieldEvent e = new FieldEvent("SkipQuestion", true);
        FieldEvent e2 = new FieldEvent("SkipQuestion", true);
        AddCard(0, e);
        AddCard(0, e2);
    }

    public void AddCard(int playerNumber, FieldEvent eventC) {
        eventsInventory[playerNumber].Add(eventC);
        UpdateInventoryUI(playerNumber);
    }

    public void RemoveCard(int playerNumber, FieldEvent eventC) {
        eventsInventory[playerNumber].Remove(eventC);
        UpdateInventoryUI(playerNumber);
    }

    void UpdateInventoryUI(int playerNumber)
    {
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (FieldEvent eventC in eventsInventory[playerNumber])
        {
            GameObject newText = Instantiate(textPrefab, panel.transform);
            TextMeshProUGUI textComponent = newText.GetComponent<TextMeshProUGUI>();
            Button buttonComponent = newText.GetComponent<Button>();


            if (textComponent != null)
            {
                textComponent.text = eventC.GetEventType();
            }
            else
            {
                Debug.LogError("Text component not found on the instantiated prefab.");
            }

            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnCardClicked(playerNumber, eventC));
            }
            else
            {
                Debug.LogError("Button component not found on the instantiated prefab.");
            }
        }
    }

    void OnCardClicked(int playerNumber, FieldEvent fieldEvent)
    {
        Debug.Log(fieldEvent.GetEventType() + " triggered by Player " + playerNumber);
        RemoveCard(playerNumber, fieldEvent);
    }
}
