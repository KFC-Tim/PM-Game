using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreboardManager : MonoBehaviour
{
    public GameObject scoreboardPanel; 
    public GameObject scoreboardItemPrefab; 

    private List<Text> playerScoreItems = new List<Text>();

    public void AddPlayer(string playerName)
    {
        GameObject newScoreItem = Instantiate(scoreboardItemPrefab, scoreboardPanel.transform);
        
        Text textComponent = newScoreItem.AddComponent<Text>();
        textComponent.text = playerName + ": 0";
        textComponent.fontSize = 24;
        textComponent.color = Color.black;

        playerScoreItems.Add(textComponent);
    }

    public void SetScore(string playerName, int score)
    {
        foreach (var item in playerScoreItems)
        {
            if (item.text.StartsWith(playerName))
            {
                item.text = playerName + ": " + score;
                return;
            }
        }
        Debug.LogWarning("Player not found in scoreboard: " + playerName);
    }
}
