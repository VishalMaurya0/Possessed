using System.Collections.Generic;
using UnityEngine;

public class PropID : MonoBehaviour
{
    public int propID;
    public List<Transform> positions;

    private void OnEnable() {
        ItemSpawningInPropsManager.instance.MakeItemSpawningInPropsManagerRuntimeData(this);
    }
}
