using UnityEngine;

public class NoiseHandler : MonoBehaviour
{
    [Header("Noise Settings")]
    public float noiseValue;
    public float positionPresitionRadius = 200;

    [Header("All Noises Value")]
    public float footNoise;
    public float instrumentNoise;
    public float VoiceNoise;

    [Header("All Noise Data")]
    public NoiseData footNoiseData;
    public NoiseData instrumentNoiseData;

    [Header("Foot Noise Calculation")]
    private float timeDurationTimer;
    Vector3 initianPos;
    public Vector3 currentPos;  


    [Header("Ghost Data")]
    GhostData ghostData;
    GhostAI ghostAI;

    [Header("References")]
    Inventory inventory;
    public PlayerDataSO playerData;
    

    private void Awake()
    {
        ghostAI = FindAnyObjectByType<GhostAI>();
        ghostData = ghostAI.ghostData;
        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        CalculateInstrumentNoise();
        noiseValue = footNoise + instrumentNoise + VoiceNoise;
        
    }
    private void FixedUpdate()
    {
        CalculateFootNoise();
    }


    private void CalculateFootNoise()
    {
        timeDurationTimer += Time.deltaTime;
        if (timeDurationTimer > playerData.timeDurationForCalculatingFootNoise)
        {
            currentPos = transform.position;
            if ((currentPos - initianPos).magnitude >= playerData.walkDist)
            {
                Debug.Log((currentPos-initianPos).magnitude);
                footNoise = CalculateNoise(footNoiseData);
            }else
            {
                footNoise = 0;
            }
            initianPos = transform.position;
            timeDurationTimer = 0;
        }
    }



    private void CalculateInstrumentNoise()
    {
        if (inventory.selectedInventorySlot.itemData != null && !inventory.selectedInventorySlot.itemData.isOn)
        {
            instrumentNoise = 0;
            return;
        }

        instrumentNoise = CalculateNoise(instrumentNoiseData);
    }











    
    float CalculateNoise(NoiseData noiseData)
    {
        float noise = 0;
        float distanceFromGhostSquared = (ghostAI.transform.position - transform.position).sqrMagnitude;

        if (distanceFromGhostSquared < noiseData.radiusFor100 * noiseData.radiusFor100)
        {
            noise = noiseData.noiseValue100;
        }
        else if (distanceFromGhostSquared < noiseData.radiusFor75 * noiseData.radiusFor75)
        {
            noise = noiseData.noiseValue100 * .75f;
        }
        else if (distanceFromGhostSquared < noiseData.radiusFor50 * noiseData.radiusFor50)
        {
            noise = noiseData.noiseValue100 * .5f;
        }
        else if (distanceFromGhostSquared < noiseData.radiusFor25 * noiseData.radiusFor25)
        {
            noise = noiseData.noiseValue100 * .25f;
        }

        return noise;
    }

}
