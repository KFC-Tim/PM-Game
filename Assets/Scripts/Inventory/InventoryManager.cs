using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject textPrefab;

    private List<FieldEvent> events = new List<FieldEvent>();

    // Start is called before the first frame update
    void Start()
    {
        FieldEvent e = new FieldEvent("SkipQuestion", true);
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
            newText.GetComponent<Text>().text = eventC.GetEventType();
        }
    }
}
