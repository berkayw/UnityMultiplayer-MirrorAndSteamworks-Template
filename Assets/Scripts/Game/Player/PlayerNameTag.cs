using TMPro;
using UnityEngine;
using Steamworks;
using Mirror;

public class PlayerNameTag : NetworkBehaviour
{
    public TextMeshProUGUI nameTag; 

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName; 

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            CmdSetPlayerName(SteamClient.Name);
        }
    }

    [Command]
    void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    void OnNameChanged(string oldName, string newName)
    {
        nameTag.text = newName; 
    }

    private void LateUpdate()
    {
        if (nameTag != null && Camera.main != null)
        {
            Vector3 direction = Camera.main.transform.position - nameTag.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                nameTag.transform.rotation = Quaternion.LookRotation(-direction);
            }
        }
    }
}