using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject textPrefab;
    
    private List<FieldEvent>[] eventsInventory = new List<FieldEvent>[4];
    private FieldEvent [] currentEvent = {null, null, null, null};
    

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.name + " loaded!");
        HideInventory();
        eventsInventory[0] = new List<FieldEvent>();
        eventsInventory[1] = new List<FieldEvent>();
        eventsInventory[2] = new List<FieldEvent>();
        eventsInventory[3] = new List<FieldEvent>();
        
        FieldEvent e = new FieldEvent("SkipQuestion", true);
        FieldEvent e2 = new FieldEvent("SkipQuestion", true);
        AddCard(0, e);
        AddCard(0, e2);
    }

    public void HideInventory()
    {
        panel.SetActive(false);
    }
    
    public void ShowInventory()
    {
        panel.SetActive(true);
    }

    public void AddCard(int playerNumber, FieldEvent eventC) {
        eventsInventory[playerNumber].Add(eventC);
        UpdateInventoryUI(playerNumber);
    }

    public void RemoveCard(int playerNumber, FieldEvent eventC) {
        eventsInventory[playerNumber].Remove(eventC);
        UpdateInventoryUI(playerNumber);
    }

    public void UpdateInventoryUI(int playerNumber)
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
        if (currentEvent[playerNumber] != null)
        {
            Debug.Log("Cant perform Event because others player event will be done first");
        }
        currentEvent[playerNumber] = fieldEvent;
        RemoveCard(playerNumber, fieldEvent);
    }

    public FieldEvent GetCurrentEvent(int playerNumber)
    {
        return currentEvent[playerNumber];
    }

    public void ClearCurrentEvent(int playerNumber)
    {
        currentEvent[playerNumber] = null;
    }
}
