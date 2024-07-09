using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScoreboardManager : MonoBehaviour
{
    //private TMP_Text[] playerNames = new TMP_Text[4];
    //private TMP_Text[] playerScores = new TMP_Text[4];

    private int maxPlayer = 4;
    public GameObject canvasScoreboard;

    public void InitializeScoreboard(string[] playerNamesArray, string[] playerUUIDsArray,  Dictionary<string, int> scoreBoard)
    {


        TMP_Text[] playerNames = new TMP_Text[4];
        TMP_Text[] playerScores = new TMP_Text[4];


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
                
                setColor(i, playerNames[i]);
            }

            foreach(KeyValuePair<string, int> kvp in scoreBoard)
            {
                if (kvp.Key == playerUUIDsArray[i]){
                    playerScores[i].text = kvp.Value.ToString();
                    setColor(i, playerScores[i]);
                }
            }
        }

    }


    private void setColor(int index, TMP_Text text)
    {
        switch (index)
        {
            case 0:
                text.color = Color.red; break;
            case 1:
                text.color = Color.blue; break;
            case 2:
                text.color = Color.yellow; break;
            case 3:
                text.color = Color.green; break;
        }
    }
}
    
