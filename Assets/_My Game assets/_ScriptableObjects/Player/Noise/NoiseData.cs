using UnityEngine;

[CreateAssetMenu(fileName = "NoiseData", menuName = "Scriptable Objects/NoiseData")]
public class NoiseData : ScriptableObject
{
    public float noiseValue100 = 60f;


    public float radiusFor100 = 5f;
    public float radiusFor75 = 10f;
    public float radiusFor50 = 25f;
    public float radiusFor25 = 50f;
}
