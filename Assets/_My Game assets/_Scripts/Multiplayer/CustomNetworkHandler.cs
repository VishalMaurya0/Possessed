using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SessionTagManager : MonoBehaviour
{
    private string lobbyId;

    public async Task FetchLobbyId()
    {
        try
        {
            var lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();
            if (lobbyIds.Count > 0)
            {
                lobbyId = lobbyIds[0];
                Debug.LogWarning("lobby found.");
                Debug.LogWarning(lobbyId);
                // Now you have the full lobby object
            }
            else
            {
                Debug.LogWarning("No joined lobbies found.");
            }

        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"[LobbyIdFetcher] Failed to get joined lobby: {ex.Message}");
        }
    }


    public async void TogglePrivatePublic(bool isPrivate)
    {
        await FetchLobbyId();
        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("Lobby ID not set. Use SetLobbyId() after session is created.");
            return;
        }

        var options = new UpdateLobbyOptions
        {
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                {
                    "visibility",
                    new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: isPrivate ? "private" : "public",
                        index: DataObject.IndexOptions.S1
                    )
                }
            }
        };

        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
            Debug.Log($"Lobby visibility updated to {(isPrivate ? "private" : "public")}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to update lobby: {ex.Message}");
        }
    }
}
