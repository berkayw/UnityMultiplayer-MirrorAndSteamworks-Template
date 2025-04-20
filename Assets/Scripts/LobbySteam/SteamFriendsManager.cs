using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SteamFriendsManager : MonoBehaviour
{
    public RawImage profilePicture;
    public TextMeshProUGUI playerName;

    public Transform friendsContent;
    public GameObject friendPrefab;
    
    private async void Start()
    {
        if(!SteamClient.IsValid) return;

        playerName.text = SteamClient.Name;

        InitFriends();
        

        profilePicture.texture = await GetTextureFromSteamIDAsync(SteamClient.SteamId);

    }

    private void InitFriends()
    {
        foreach (var friend in SteamFriends.GetFriends())
        {
            GameObject f = Instantiate(friendPrefab, friendsContent);
            
            f.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
            f.GetComponent<FriendObject>().friendSteamID = friend.Id;
            AssignFriendImage(f, friend.Id);
        }
    }

    public static async void AssignFriendImage(GameObject f, SteamId friendID)
    {
        f.GetComponentInChildren<RawImage>().texture = await GetTextureFromSteamIDAsync(friendID);
    }
    
    public static async Task<Texture2D> GetTextureFromSteamIDAsync(SteamId ID)
    {
        var img = await SteamFriends.GetLargeAvatarAsync(ID);
        Steamworks.Data.Image image = img.Value;
        Texture2D texture = new Texture2D((int) image.Width, (int) image.Height);
        
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x,y);
                texture.SetPixel(x, (int)image.Height - y, new Color(p.r / 255.0f, p.g / 255.0f, p.b /255.0f, p.a / 255.0f));
            }
        }
        texture.Apply();
        
        return texture;
    }

    


}
