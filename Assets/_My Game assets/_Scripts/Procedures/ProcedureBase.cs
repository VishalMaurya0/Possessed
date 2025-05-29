using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class ProcedureBase : NetworkBehaviour
{
    public List<ProcedureBase> allProcedures = new();

    public GameObject CompletionVFX;
    public List<Vector3> position = new();
    public List<Vector3> rotation = new();


    private void Awake()
    {
        for (int i = 0; i < 8; i++)
        {
            allProcedures.Add(null);
            position.Add(Vector3.zero);
            rotation.Add(Vector3.zero);
        }
    }

    public void Completed(Vector3 position)
    {
        GameObject obj = Instantiate(CompletionVFX, position, Quaternion.identity);
        StartCoroutine(DestroyAfterEffect(obj));
    }

    IEnumerator DestroyAfterEffect(GameObject obj)
    {
        var vfx = obj.GetComponent<VisualEffect>();

        yield return new WaitForSeconds(vfx.GetFloat("Duration"));

        // Stop new particles from spawning
        vfx.SetFloat("SpawnRate", 0);

        yield return new WaitForSeconds(2f);
        Destroy(obj);
    }

}


[System.Serializable]
public struct TotalItemsNeeded //========= For Completing Procedure ======//
{
    public List<ItemNeeded> itemNeeded;
    public List<int> addedAmount;
}