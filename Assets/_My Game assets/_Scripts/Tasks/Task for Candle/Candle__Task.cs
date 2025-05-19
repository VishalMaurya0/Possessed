using System;
using Unity.Netcode;
using UnityEngine;

public class Candle__Task : NetworkBehaviour
{
    [Header("References")]
    public CandleTask CandleTask;
    public ParticleSystem fire;
    public Material unlittingMat;

    public NetworkVariable<bool> isLit = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> active = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    [Header("For Glow")]
    public MeshRenderer G1;
    public MeshRenderer G2;
    public Material normal1;
    public Material normal2;
    public Material glow;
    private void Start()
    {
        CandleTask = GetComponentInParent<CandleTask>();
        normal1 = G1.sharedMaterial;
        normal2 = G2.sharedMaterial;
    }

    private void OnMouseEnter()
    {
        G1.material = glow;
        G2.material = glow;
    }

    private void OnMouseExit()
    {
        SetNormalMat();
    }


    public void SetNormalMat()
    {
        G2.material = normal2;
        G1.material = normal1;
    }

    private void OnMouseUp()
    {
        MouseClickServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MouseClickServerRpc()
    {
        if (!CandleTask.gameStarted)
        {
            CandleTask.StartGame();
            return;
        }

        if (!isLit.Value && active.Value)
        {
            isLit.Value = true;
            fire.Play();
            active.Value = false;
            CandleTask.Correct();
            SetNormalMat();
            return;
        }

        if (!isLit.Value && !active.Value)
        {
            CandleTask.InCorrect();
            foreach (var candle in CandleTask.allCandles)
            {
                candle.isLit.Value = true;
                candle.fire.Play();
                candle.active.Value = false;
            }
            SetNormalMat();
            return;
        }

        if (isLit.Value && CandleTask.currentIteration == 0)
        {
            CandleTask.InCorrect();
            return;
        }
    }
}


