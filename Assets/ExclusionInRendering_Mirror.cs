using System.Collections.Generic;
using UnityEngine;

public class ExclusionInRendering_Mirror : MonoBehaviour
{
    Dictionary<GameObject, int> layerChanged = new();
    public LayerMask mask;

    private void OnTriggerEnter(Collider other)
    {
        if (!layerChanged.ContainsKey(other.gameObject))
        {
            layerChanged.Add(other.gameObject, other.gameObject.layer);
            other.gameObject.layer = Mathf.RoundToInt(Mathf.Log(mask.value, 2));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (layerChanged.ContainsKey(other.gameObject))
        {
            other.gameObject.layer = layerChanged[other.gameObject];
            layerChanged.Remove(other.gameObject);
        }
    }
}
