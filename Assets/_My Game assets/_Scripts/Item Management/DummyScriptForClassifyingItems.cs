using UnityEngine;

public class DummyScriptForClassifyingItems : MonoBehaviour
{
    public ItemData ItemData;
    ItemDataSO ItemDataSO;
    public Transform toFollow;
    public float followSpeed = 10f;
    public bool makeItSpringy = true;
    // Tune these:
    public float stiffness = 900f;      // higher = stronger spring force
    public float damping = 50f;         // higher = less oscillation; 2 * sqrt(stiffness) = critical damping;
    private Vector3 lastPos;

    private void Start()
    {
        ItemDataSO = ScriptableObjectFinder.Instance.FindItemSO(ItemData);
    }

    private Vector3 velocity;

    private void FixedUpdate()
    {
        if (toFollow == null) return;

        if (!makeItSpringy)
        {
            transform.position = toFollow.position;
            return;
        }

        Vector3 target = toFollow.position;

        Vector3 displacement = target - transform.position;
        Vector3 springForce = displacement * stiffness;
        Vector3 dampingForce = -velocity * damping;

        Vector3 force = springForce + dampingForce;

        velocity += force * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }

    private void LateUpdate()
    {
        if (toFollow == null) return;

        float smoothStep = 1 - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, toFollow.rotation, smoothStep);
    }

    private void Update()
    {
        if (!transform.GetChild(ItemData.currentState).gameObject.activeSelf)
        {
            SetActiveGameobjState(ItemData.currentState);
        }
    }

    public void SetActiveGameobjState(int state)
    {
        for (int i = 0; i < ItemDataSO.noOfStates; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        transform.GetChild(state).gameObject.SetActive(true);
    }
}
