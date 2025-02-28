using Unity.Netcode;
using UnityEngine;

public class FireScriptForVoodooDoll : NetworkBehaviour
{

    [SerializeField] TaskVoodooDoll taskVoodooDoll__parent;
    bool activated = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        taskVoodooDoll__parent = GetComponentInParent<TaskVoodooDoll>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (!IsServer) return;
        if (collision.gameObject.CompareTag("Doll") && activated)
        {
            activated = false;
            Debug.Log("Collided with Doll!");
            NetworkObject doll = collision.gameObject.GetComponent<NetworkObject>();
            if (doll != null)
            {
                Debug.Log("Despawn Doll!");
                doll.Despawn();
            }
            else
            {
                Debug.LogError("NetworkObject missing on Doll!");
            }
            taskVoodooDoll__parent.dollsAdded++;
        }
    }

}
