using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Animator mainMenuAnimator;
    public Animator MultiplayerMenuAnimator;


    public void LoadMainMenu()
    {
        mainMenuAnimator.SetBool("Load", true);
    }
    
    public void DeLoadMainMenu()
    {
        mainMenuAnimator.SetBool("Load", false);
    }
    
    public void LoadMultiplayerMenu()
    {
        MultiplayerMenuAnimator.SetBool("Load", true);
    }
    
    public void DeLoadMultiplayerMenu()
    {
        MultiplayerMenuAnimator.SetBool("Load", false);
    }
}
