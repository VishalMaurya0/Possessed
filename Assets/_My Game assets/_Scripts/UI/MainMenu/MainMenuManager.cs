using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Animator mainMenuAnimator;
    public Animator MultiplayerMenuAnimator;
    public Animator HostGamePanelAnimator;
    public Animator JoinGamePanelAnimator;
    public Animator LobbyPanelAnimator;


    private void Update()
    {
        if (!HostGamePanelAnimator.enabled)
        {
            Debug.LogWarning("Animator got disabled!");
            HostGamePanelAnimator.enabled = true;  // Re-enable it
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
    
}
