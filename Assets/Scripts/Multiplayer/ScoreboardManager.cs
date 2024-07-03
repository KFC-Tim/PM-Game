using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScoreboardManager : MonoBehaviour
{
    private TMP_Text[] playerNames = new TMP_Text[4];
    private TMP_Text[] playerScores = new TMP_Text[4];

    public void InitializeScoreboard(string[] playerNamesArray, string[] playerUUIDsArray,  Dictionary<string, int> scoreBoard)
    {

        if (playerNamesArray == null || playerUUIDsArray == null)
        {
            Debug.LogError("Player names or UUIDs array is null");
            return;
        }

        int playerCount = playerNamesArray.Length;

        for (int i = 0; i < playerCount; i++)
        {
            playerNames[i] = GameObject.Find($"PlayerName{i + 1}").GetComponent<TMP_Text>();
            playerScores[i] = GameObject.Find($"PlayerScore{i + 1}").GetComponent<TMP_Text>();

            if (playerNames[i] == null)
            {
                Debug.LogError($"playerNames[{i}] is not assigned");
            }
            else
            {
                playerNames[i].text = playerNamesArray[i];
            }

            foreach(KeyValuePair<string, int> kvp in scoreBoard)
            {
                if (kvp.Key == playerUUIDsArray[i]){
                    playerScores[i].text = kvp.Value.ToString();
                }
            }
        }
    }
    
}
