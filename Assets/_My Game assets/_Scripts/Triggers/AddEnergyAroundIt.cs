using UnityEngine;

public class AddEnergyAroundIt : MonoBehaviour
{
    public GameObject energyPrefab;
    public float roamSpeed = 2f;
    public float roamRange = 5f;

    public bool isEnergyActive;
    public float energyRange = 20;
    public float maxEnergy = 50f;

    public bool isEMFActive;
    public float EMFRange = 20;
    public float maxEMFEnergy = 50f;

    public bool isTemperatureActive;
    public float temperatureRange = 20;
    public float maxTemperature = 15;

    public bool isPressureActive;
    public float pressureRange = 20;
    public float maxPressure = 10;

    private GameObject energyGameObj;

    private EnergyTrigger energyTrigger;
    private EMFTrigger emfTrigger;
    private TemperatureTrigger temperatureTrigger;
    private PressureTrigger pressureTrigger;

    private Vector3 currentTargetOffset;

    private float energyAmount;
    private float emfAmount;
    private float temperatureAmount;
    private float pressureAmount;

    private float fluctuationTimer = 0f;
    private float fluctuationInterval = 3f;

    private bool hasSpawned = false;

    private void Start()
    {
        if (!hasSpawned)
            SpawnEnergyObject();

        GameManager.onServerStarted += GameManager_onServerStarted;
    }

    private void GameManager_onServerStarted()
    {
        if (!hasSpawned)
            SpawnEnergyObject();
    }

    private void SpawnEnergyObject()
    {
        if (hasSpawned) return;

        hasSpawned = true;

        energyGameObj = Instantiate(energyPrefab);
        energyGameObj.hideFlags = HideFlags.DontSave;
        energyTrigger = energyGameObj.AddComponent<EnergyTrigger>();
        emfTrigger = energyGameObj.AddComponent<EMFTrigger>();
        temperatureTrigger = energyGameObj.AddComponent<TemperatureTrigger>();
        pressureTrigger = energyGameObj.AddComponent<PressureTrigger>();

        var collider = energyGameObj.GetComponent<SphereCollider>();
        if (collider != null)
            collider.radius = energyRange;

        PickNewTarget();
    }


    private void Update()
    {
        if (energyGameObj == null) return;

        MoveEnergyAtConstantSpeed();
        UpdateFluctuations();

        // Set values on triggers
        energyTrigger.isActive = isEnergyActive;
        energyTrigger.EnergyAmount = energyAmount;
        energyTrigger.energyRange = energyRange;

        emfTrigger.isActive = isEMFActive;
        emfTrigger.Amount = emfAmount;
        emfTrigger.Range = EMFRange;

        temperatureTrigger.isActive = isTemperatureActive;
        temperatureTrigger.Amount = temperatureAmount;
        temperatureTrigger.Range = temperatureRange;

        pressureTrigger.isActive = isPressureActive;
        pressureTrigger.Amount = pressureAmount;
        pressureTrigger.Range = pressureRange;
    }

    private void UpdateFluctuations()
    {
        fluctuationTimer += Time.deltaTime;
        if (fluctuationTimer >= fluctuationInterval)
        {
            fluctuationTimer = 0f;

            float randomFactor = Random.Range(0f, 1f);
            float biased = Mathf.Pow(randomFactor, 2); // Bias toward lower values

            energyAmount = biased * maxEnergy;
            emfAmount = biased * maxEMFEnergy;
            temperatureAmount = biased * maxTemperature;
            pressureAmount = biased * maxPressure;
        }
    }

    private void MoveEnergyAtConstantSpeed()
    {
        Vector3 targetWorldPos = transform.position + currentTargetOffset;
        Vector3 currentPos = energyGameObj.transform.position;

        Vector3 direction = (targetWorldPos - currentPos).normalized;
        float distanceToTarget = Vector3.Distance(currentPos, targetWorldPos);

        if (distanceToTarget < 0.1f)
        {
            PickNewTarget();
        }
        else
        {
            energyGameObj.transform.position += direction * roamSpeed * Time.deltaTime;
        }
    }

    private void PickNewTarget()
    {
        currentTargetOffset = new Vector3(
            Random.Range(-roamRange, roamRange),
            0f,
            Random.Range(-roamRange, roamRange)
        );
    }

    private void OnDestroy()
    {
        if (energyGameObj != null)
        {
            Destroy(energyGameObj);
        }
    }
}

// Trigger components
public class EnergyTrigger : MonoBehaviour
{
    public float EnergyAmount;
    public bool isActive;
    public float energyRange;
}

public class EMFTrigger : MonoBehaviour
{
    public float Amount;
    public bool isActive;
    public float Range;
}

public class TemperatureTrigger : MonoBehaviour
{
    public float Amount;
    public bool isActive;
    public float Range;
}

public class PressureTrigger : MonoBehaviour
{
    public float Amount;
    public bool isActive;
    public float Range;
}
