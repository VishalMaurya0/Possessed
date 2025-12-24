using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : NetworkBehaviour
{
    public Animator mainMenuAnimator;
    public Animator MultiplayerMenuAnimator;
    public Animator HostGamePanelAnimator;
    public Animator JoinGamePanelAnimator;
    public Animator LobbyPanelAnimator;
    public Animator ChooseColorPanelAnimator;
    public Animator ChooseColorButtonAnimator;

    public LogListener logListener;
    public SetPlayerName setPlayerName;


    private void OnEnable()
    {
        PrivateAndPublicLobbyManager.OnAllPlayersAreActiveInLobby += RemoveLoadingScreen;
        logListener.OnJoinFailed += JoinFailed;
    }

    public void OnDisable()
    {
        PrivateAndPublicLobbyManager.OnAllPlayersAreActiveInLobby -= RemoveLoadingScreen;
        logListener.OnJoinFailed -= JoinFailed;
    }

    private void JoinFailed()
    {
        Debug.LogWarning("Join Failed triggered in MenuManager!");  //TODO show text on screen
        RemoveLoadingScreen();
        LoadMultiplayerMenu(true);
        LoadLobbyPanel(false);
    }

    public void RemoveLoadingScreen()
    {
        GameManager.Instance.ShowLoadingPanel(false);
    }

    public void ShowLoadingScreen()
    {
        Debug.Log("Toggling Loading Panel: " + true);
        GameManager.Instance.ShowLoadingPanel(true);
    }


    private void Update()
    {
        if (!HostGamePanelAnimator.enabled)
        {
            Debug.LogWarning("Animator got disabled!");
            HostGamePanelAnimator.enabled = true;  // Re-enable it
        }
//#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Escape))  //====Testing
        {
            LoadMainMenu(true);
            LoadHostGamePanel(false);
            LoadMultiplayerMenu(false);
            LoadJoinGamePanel(false);
            LoadLobbyPanel(false);
            setPlayerName.LeaveCurrentSession();
        }
//#endif
    }
    public void LoadMainMenu(bool load)
    {
        mainMenuAnimator.SetBool("Load", load);
    }
    
    
    public void LoadMultiplayerMenu(bool load)
    {
        MultiplayerMenuAnimator.SetBool("Load", load);
    }
    
    
    public void LoadHostGamePanel(bool load)
    {
        HostGamePanelAnimator.SetBool("Load", load);
    }
    
    public void LoadJoinGamePanel(bool load)
    {
        JoinGamePanelAnimator.SetBool("Load", load);
    }
    
    public void LoadLobbyPanel(bool load)
    {
        LobbyPanelAnimator.SetBool("Load", load);
    }

    public void LoadChooseColorPanel()
    {
        ChooseColorPanelAnimator.SetBool("Load", !ChooseColorPanelAnimator.GetBool("Load"));
        if (ChooseColorPanelAnimator.GetBool("Load"))
        {
            ChooseColorButtonAnimator.SetBool("Selected 0", true);
        }else
        {
            ChooseColorButtonAnimator.SetBool("Selected 0", false);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
    
}
