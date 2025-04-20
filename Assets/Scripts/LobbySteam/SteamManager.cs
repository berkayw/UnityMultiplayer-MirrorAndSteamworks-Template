using System;
using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public uint appID;
    
    private void Awake()
    {
        //DontDestroyOnLoad(this);

        try
        {
            SteamClient.Init(appID);
            Debug.Log("Steam is up and running!");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            SteamClient.Shutdown();
            Debug.Log("Steam shutdown");

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
