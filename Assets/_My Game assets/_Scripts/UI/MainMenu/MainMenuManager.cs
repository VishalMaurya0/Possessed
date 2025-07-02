using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Animator mainMenuAnimator;
    public Animator MultiplayerMenuAnimator;
    public Animator HostGamePanelAnimator;
    public Animator JoinGamePanelAnimator;
    public Animator LobbyPanelAnimator;
    public Animator ChooseColorPanelAnimator;
    public Animator ChooseColorButtonAnimator;


    
    private void Update()
    {
        if (!HostGamePanelAnimator.enabled)
        {
            Debug.LogWarning("Animator got disabled!");
            HostGamePanelAnimator.enabled = true;  // Re-enable it
        }

        if (Input.GetKeyUp(KeyCode.Escape))  //====Testing
        {
            LoadMainMenu(true);
            LoadHostGamePanel(false);
            LoadMultiplayerMenu(false);
            LoadJoinGamePanel(false);
            LoadLobbyPanel(false);
        }
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
