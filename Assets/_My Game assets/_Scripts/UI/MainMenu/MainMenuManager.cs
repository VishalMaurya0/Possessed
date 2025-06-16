using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Animator mainMenuAnimator;
    public Animator MultiplayerMenuAnimator;
    public Animator HostGamePanelAnimator;


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
        Debug.LogError("dfghjk");
    }
    
}
