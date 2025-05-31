using UnityEngine;

public class Procedure2_Ghost : MonoBehaviour
{
    public Procedure2Visuals procedure2Visuals;

    private void Start()
    {
        procedure2Visuals = GetComponentInParent<Procedure2Visuals>();
    }

    public void GhostSpanned()
    {
        procedure2Visuals.Jailed();
    }
}
