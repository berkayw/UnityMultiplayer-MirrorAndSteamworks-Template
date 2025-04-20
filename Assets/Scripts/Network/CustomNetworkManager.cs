using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    
    //This runs only on Client
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("client's scene changed");
    }

    //This runs only on Host-Server 
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName); 

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        SpawnAllPlayers();
        Debug.Log("host's scene changed");
    }
    
    private void SpawnAllPlayers()
    {
        if (!NetworkServer.active) return;
        NetworkClient.Ready(); //Necessary because the base.OnServerSceneChanged() don't call Ready().
        
        // spawn clients
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.identity == null)
            {
                SpawnPlayerForConnection(conn);
            }
        }
    }
    
    private void SpawnPlayerForConnection(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player); // only host calls
    }
    
    public void ChangeScene() //Calling by pressing Start Button, set from inspector.
    {
        if (NetworkServer.active)
        {
           
            // CHANGE SCENE
            Invoke("ChangingScene",3f);
            
        }
    }

    public void ChangingScene()
    {
        ServerChangeScene("GameScene");
    }

}
