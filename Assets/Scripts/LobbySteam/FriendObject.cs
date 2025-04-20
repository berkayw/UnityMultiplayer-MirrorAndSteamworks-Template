using System;
using Steamworks;
using UnityEngine;

public class FriendObject : MonoBehaviour
{
    public SteamId friendSteamID;
    
    public async void Invite()
    {
        if (SteamLobbyManager.isUserInLobby)
        {
            SteamLobbyManager.currentLobby.InviteFriend(friendSteamID);
            Debug.Log("Invited: " + friendSteamID);
        }
        else
        {
            bool result = await SteamLobbyManager.CreateLobby();
            if (result)
            {
                SteamLobbyManager.currentLobby.InviteFriend(friendSteamID);
                Debug.Log("Invited: " + friendSteamID + " Created new lobby.");
            }
        }
    }
}
