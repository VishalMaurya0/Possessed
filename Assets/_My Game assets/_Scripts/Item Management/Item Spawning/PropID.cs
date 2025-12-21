using System.Collections.Generic;
using UnityEngine;

public class PropID : MonoBehaviour
{
    public int propID;
    public List<Transform> positions;

    private void Start() {
        ItemSpawningInPropsManager.instance.MakeItemSpawningInPropsManagerRuntimeData(this);
    }
}
