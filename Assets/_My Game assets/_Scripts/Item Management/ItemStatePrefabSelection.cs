using UnityEngine;

public class ItemStatePrefabSelection : MonoBehaviour
{
    ItemPickup itemPickup;
    ItemData itemData;
    ItemDataSO itemDataSO;

    private void Start()
    {
        itemPickup = GetComponent<ItemPickup>();
        itemData = itemPickup.itemData;
        itemDataSO = itemPickup.ItemDataSO;
    }

    private void Update()
    {
        if (!transform.GetChild(itemData.currentState).gameObject.activeSelf)
        {
            SetActiveGameobjState(itemData.currentState);
        }
    }

    public void SetActiveGameobjState(int state)
    {
        for (int i = 0; i < itemDataSO.noOfStates; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        transform.GetChild(state).gameObject.SetActive(true);
    }
}
