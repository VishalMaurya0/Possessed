using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using Unity.Netcode;

public class PrivateAndPublicLobbyManager : NetworkBehaviour
{
    private string lobbyId;
    public Toggle isPrivate_ToggleWhileHostingGame;
    public bool isPrivate = true;

    void Start()
    {
        isPrivate_ToggleWhileHostingGame.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        isPrivate = isOn;
    }

    public void OnLobbyCreated()
    {
        if (IsServer)
        TogglePrivatePublic(isPrivate);
    }

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
        if (!IsServer)
        {
            return;
        }
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
