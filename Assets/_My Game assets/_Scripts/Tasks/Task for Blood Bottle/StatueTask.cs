using Unity.Netcode;
using UnityEngine;

public class StatueTask : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] ChestUnlock_BloodBottleTask chestUnlock_BloodBottleTask;
    

    public NetworkVariable<int> value = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] int rotationSpeed = 3;

    void Start()
    {
        value.Value = 0;
        GameObject table = transform.parent.parent.gameObject;
        chestUnlock_BloodBottleTask = table.GetComponentInChildren<ChestUnlock_BloodBottleTask>();
    }


    void Update()
    {
        if (!IsServer) { return; }
        Rotate___codeRunningOnServer();
    }

    private void OnMouseDown()
    {
        ChangeValueByOneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeValueByOneServerRpc()
    {
        value.Value++;
        if (value.Value >= 4)
        {
            value.Value = 0;
        }
        chestUnlock_BloodBottleTask.UpdateCurrentCode();
    }

    private void Rotate___codeRunningOnServer()
    {
        transform.localRotation = Quaternion.Euler( 
            Vector3.Lerp(
                transform.localRotation.eulerAngles, 
                new Vector3(0f, value.Value * 90f, 0f), 
                Time.deltaTime * rotationSpeed
            )
        );
    }
}
