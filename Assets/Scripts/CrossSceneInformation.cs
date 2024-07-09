using System;
using Unity.VisualScripting;
using UnityEngine;

public class CrossSceneInformation
{
    public static MultiplayerManager MultiplayerManager { get; set; }
    public static bool GameMasterLoaded { get; private set; }
    public static int currentPlayers;

    public static bool GetGameMasterLoaded()
    {
        return GameMasterLoaded;
    }

    public static void SetGameMasterLoaded(bool value)
    {
        Debug.Log("GameMasterLoaded boolean set!");
        GameMasterLoaded = value;
    }
    public static GameMaster GameMasterInstance { get; private set; }
    
    public static bool GetGameMasterInstance()
    {
        return GameMasterInstance;
    }

    public static void SetGameMasterInstance(GameMaster gameMaster)
    {
        Debug.Log("GameMasterInstance set");
        MultiplayerManager.GetInstance().SetGameMaster(gameMaster);
    }

     private static float musicVolume = 1.0f;
    
    public static float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            Debug.Log($"Music volume set to: {musicVolume}");
        }
    }

}
