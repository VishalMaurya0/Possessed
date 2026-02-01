using UnityEngine;

public class Procedure2_Ghost : MonoBehaviour
{
    public Procedure2Visuals procedure2Visuals;

    private void Start()
    {
        procedure2Visuals = GetComponentInParent<Procedure2Visuals>();
    }

    public void GhostSpannedAndJailed()
    {
        procedure2Visuals.Jailed();
        
        //AudioManager.PlaySound(AudioType.GhostRoar);
    }

    public void JailSound()
    {
        AudioManager.PlaySound(AudioType.MysticalClick);
    }
    
    public void GhostRoarSound()
    {
        AudioManager.PlaySound(AudioType.GhostRoar);
    }
}
