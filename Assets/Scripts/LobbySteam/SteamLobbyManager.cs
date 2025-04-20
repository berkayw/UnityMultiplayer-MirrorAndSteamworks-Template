using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager instance;
    
    public static Lobby currentLobby;
    public static bool isUserInLobby; 
    
    
    public UnityEvent OnCreatedLobby; 
    public UnityEvent OnJoinedLobby;  //Calling from inspector to make changes on UI.
    public UnityEvent OnLeavedLobby;  //Calling from inspector to make changes on UI.
    
    public GameObject InLobbyFriend;
    public Transform content;

    
    public Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();

    public GameObject startButton;
    
    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(this);

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnChatMessage += OnChatMessage;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;

    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {
        
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log(friend.Name + " joined the lobby.");
        GameObject friendObj = Instantiate(InLobbyFriend, content);
        inLobby.Add(friend.Id, friendObj);
        
        friendObj.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
        SteamFriendsManager.AssignFriendImage(friendObj, friend.Id);
    }
    
    private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        Debug.Log(friend.Name + " left the lobby.");
        Debug.Log( currentLobby.Owner + " is the new owner.");
        
        if (inLobby.ContainsKey(friend.Id))
        {
            Destroy(inLobby[friend.Id]);
            inLobby.Remove(friend.Id);
        }
        
    }
   
    private void OnChatMessage(Lobby lobby, Friend friend, string message)
    {
        Debug.Log("Incoming message from " + friend.Name + ": " + message);
    }

    private async void OnGameLobbyJoinRequest(Lobby joinedLobby, SteamId ID)
    {
        RoomEnter joinedLobbySuccess = await joinedLobby.Join();

        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Debug.Log("Failed to join lobby: " + joinedLobbySuccess);
        }
        else
        {
            currentLobby = joinedLobby;
        }
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log("Lobby creation result is not OK: " + result);
        }
        else
        {
            OnCreatedLobby.Invoke();
            Debug.Log("Server başladı, Host: " + currentLobby.Owner.Name);
            currentLobby.SetPublic();
            currentLobby.SetJoinable(true);
            Debug.Log("Lobby creation result is OK");
            
            CustomNetworkManager.singleton.StartHost();
            Debug.Log("mirror host started.");

            startButton.SetActive(true);
        }
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        Debug.Log("Client joined the lobby");
        ClearLobbyTool();
        
        GameObject clientObj = Instantiate(InLobbyFriend, content);
        inLobby.Add(SteamClient.SteamId, clientObj);
        
        clientObj.GetComponentInChildren<TextMeshProUGUI>().text = SteamClient.Name;
        SteamFriendsManager.AssignFriendImage(clientObj, SteamClient.SteamId);
        
        //Add existing players that joined before you
        foreach (var friend in currentLobby.Members)
        {
            if (friend.Id != SteamClient.SteamId)
            {
                GameObject clientObj2 = Instantiate(InLobbyFriend, content);
                inLobby.Add(friend.Id, clientObj2);
        
                clientObj2.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
                SteamFriendsManager.AssignFriendImage(clientObj2, friend.Id);
            }
        }
        
        OnJoinedLobby.Invoke();

        isUserInLobby = true;

        if (NetworkServer.active) return;

        CustomNetworkManager.singleton.networkAddress = currentLobby.Owner.Id.ToString();
        CustomNetworkManager.singleton.StartClient();
        
        Debug.Log("mirror client started.");
        
    }
    
    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log(friend.Name + " invited you to his lobby");
    }
    
    public async void CreateLobbyAsync()
    {
        bool lobbyCreateResult = await CreateLobby();

        if (!lobbyCreateResult)
        {
            //Invoke a error etc. if you want
        }
    } //Calling from inspector -> CreateLobbyButton

    public static async Task<bool> CreateLobby()
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
            if (!createLobbyOutput.HasValue)
            {
                Debug.Log("Lobby created but not correctly instantiated.");
                return false;
            }
            
            currentLobby = createLobbyOutput.Value;
            //currentLobby.SetPublic();
            //currentLobby.SetJoinable(true);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to create lobby: " + e.Message);
            return false;
        }
    }

    public void LeaveLobby()
    {
        try
        {
            int lobbyMemberCount = currentLobby.MemberCount;
            currentLobby.Leave();
            OnLeavedLobby.Invoke();
            ClearLobbyTool();
            
            isUserInLobby = false;

            //if you are host
            if (NetworkServer.active)
            {
                startButton.SetActive(false);
                if (lobbyMemberCount - 1 > 0)
                {
                    //change host
                    Debug.LogWarning("change host");
                    CustomNetworkManager.singleton.StopHost();//make it change later

                }
                else
                {
                    //stop host
                    Debug.Log("stop host");
                    CustomNetworkManager.singleton.StopHost();
                }
            }
            else
            {
                CustomNetworkManager.singleton.StopClient();
            }
            
        }
        catch (Exception e)
        {
            Debug.Log("Error with leaving lobby: " + e.Message);
        }
    } //Calling from inspector -> LeaveLobbyButton

    private void ClearLobbyTool()
    {
        foreach (var friendPrefab in inLobby.Values)
        {
            Destroy(friendPrefab);
        }
        inLobby.Clear();
    }

    

    
}
