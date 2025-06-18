//using UnityEngine;
//using Unity.Services.Authentication;
//using System.Collections;

//public class SetPlayerNameFromPrefs : MonoBehaviour
//{
//    IEnumerator Start()
//    {
//        // Wait until the user is signed in
//        while (!AuthenticationService.Instance.IsSignedIn)
//        {
//            yield return null; // wait one frame
//        }

//        // Now set player name
//        string playerName = PlayerPrefs.GetString("PlayerName", "Player" + Random.Range(1000, 9999));
//        SetName(playerName);
//    }

//    async void SetName(string name)
//    {
//        try
//        {
//            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
//            Debug.Log("Player name set to: " + name);
//        }
//        catch (AuthenticationException e)
//        {
//            Debug.LogWarning("Could not set player name: " + e.Message);
//        }
//    }
//}

using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Multiplayer.Widgets;
using Unity.Services.Multiplayer;



public static class LobbySessionTracker
{
    public static string LobbyId;
}





public class StartupManager : MonoBehaviour
{
    private string lastSetName = "";
    public LeaveSessionProxy leaveSessionProxy; // assign this via Inspector

    public void LeaveCurrentSession()
    {
        leaveSessionProxy.TriggerLeave();
    }

    async void Awake()
    {
        await InitializeUnityServicesAsync();
    }

    async Task InitializeUnityServicesAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        // Wait for Widgets system to sign in the player
        AuthenticationService.Instance.SignedIn += async () =>
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                lastSetName = playerName;
                Debug.Log("Player name set after sign-in: " + playerName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Could not update player name: " + e.Message);
            }
        };
    }

    void Update()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        string current = PlayerPrefs.GetString("PlayerName", "Player");
        if (current != lastSetName)
        {
            _ = AuthenticationService.Instance.UpdatePlayerNameAsync(current);
            lastSetName = current;
            Debug.Log("Player name changed and updated to: " + current);
        }
    }

}
