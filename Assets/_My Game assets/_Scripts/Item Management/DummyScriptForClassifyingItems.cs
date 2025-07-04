using UnityEngine;

public class DummyScriptForClassifyingItems : MonoBehaviour
{
    public ItemData ItemData;
    ItemDataSO ItemDataSO;
    public Transform toFollow;
    public float followSpeed = 10f;
    private Vector3 lastPos;

    private void Start()
    {
        ItemDataSO = ScriptableObjectFinder.FindItemSO(ItemData);
    }

    private void FixedUpdate()
    {
        if (toFollow == null) return;

        float smoothStep = 1 - Mathf.Exp(-followSpeed * Time.fixedDeltaTime);
        transform.position = Vector3.Lerp(transform.position, toFollow.position, smoothStep);
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
