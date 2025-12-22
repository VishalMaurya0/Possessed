using UnityEngine;

public class Procedure2_Portal : MonoBehaviour
{
    public Procedure2Visuals procedure2Visuals;

    private void Start()
    {
        procedure2Visuals = GetComponentInParent<Procedure2Visuals>();
    }

    public void PortalSpanned()
    {
        procedure2Visuals.SpawnGhost();
    }

    public void PlaySound(string soundName)
    {
        //Debug.Log("Play sound: " + soundName);
        // e.g., play sound from AudioManager
    }
}


