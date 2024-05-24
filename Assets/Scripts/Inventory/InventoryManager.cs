using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject textPrefab;

    private List<FieldEvent> events = new List<FieldEvent>();

    // Start is called before the first frame update
    void Start()
    {
        FieldEvent e = new FieldEvent("SkipQuestion", true);
        FieldEvent e2 = new FieldEvent("SkipQuestion", true);
        AddCard(e);
        AddCard(e2);
    }

    public void AddCard(FieldEvent eventC) {
        events.Add(eventC);
        UpdateInventoryUI();
    }

    public void RemoveCard(FieldEvent eventC) {
        events.Remove(eventC);
        UpdateInventoryUI();
    }

    void UpdateInventoryUI()
    {
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (FieldEvent eventC in events)
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
                buttonComponent.onClick.AddListener(() => OnCardClicked(eventC));
            }
            else
            {
                Debug.LogError("Button component not found on the instantiated prefab.");
            }
        }
    }

    void OnCardClicked(FieldEvent fieldEvent)
    {
        Debug.Log(fieldEvent.GetEventType() + " triggered!");
        RemoveCard(fieldEvent);
    }
}
