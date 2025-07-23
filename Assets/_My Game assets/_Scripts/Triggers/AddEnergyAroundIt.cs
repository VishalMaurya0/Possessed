using UnityEngine;

public class AddEnergyAroundIt : MonoBehaviour
{
    public GameObject energyPrefab;
    public float roamSpeed = 2f;
    public float roamRange = 5f;
    
    public bool isEnergyActive;
    public float energyRange = 3f;
    public float maxEnergy = 50f;

    public bool isEMFActive;
    public float EMFRange = 3f;
    public float maxEMFEnergy = 50f;

    private EnergyTrigger et;
    private EMFTrigger emft;
    private GameObject energyGameObj;
    private Vector3 currentTargetOffset;

    private void Start()
    {
        energyGameObj = Instantiate(energyPrefab);
        et = energyGameObj.AddComponent<EnergyTrigger>();
        emft = energyGameObj.AddComponent<EMFTrigger>();
        energyGameObj.GetComponent<SphereCollider>().radius = energyRange;

        PickNewTarget();

        GameManager.onServerStarted += GameManager_onServerStarted;
    }

    private void GameManager_onServerStarted()
    {
        if (et == null)
        {
            energyGameObj = Instantiate(energyPrefab);
            et = energyGameObj.AddComponent<EnergyTrigger>();
            emft = energyGameObj.AddComponent<EMFTrigger>();
            energyGameObj.GetComponent<SphereCollider>().radius = energyRange;
            PickNewTarget();
        }
    }

    private float energyAmount;
    private float emfAmount;
    private float energyChangeSpeed = 5f;
    private float fluctuationTimer = 0f;
    private float fluctuationInterval = 3f; // Time between energy fluctuations

    private void Update()
    {
        if (energyGameObj != null)
        {
            MoveEnergyAtConstantSpeed();

            et.isActive = isEnergyActive;
            et.EnergyAmount = energyAmount;
            et.energyRange = energyRange;

            emft.isActive = isEMFActive;
            emft.Amount = emfAmount;
            emft.Range = EMFRange;

            UpdateEnergyFluctuation();
        }
    }

    private void UpdateEnergyFluctuation()      //GPT
    {
        fluctuationTimer += Time.deltaTime;

        if (fluctuationTimer >= fluctuationInterval)
        {
            fluctuationTimer = 0f;

            // Simulate rare spikes by using a non-linear random chance
            float randomFactor = Random.Range(0f, 1f);
            float target = Mathf.Pow(randomFactor, 2) * maxEnergy; // Square biases toward lower values
            float emfTarget = Mathf.Pow(randomFactor, 2) * maxEMFEnergy; // Square biases toward lower values

            Debug.Log(target);

            // Smoothly interpolate energy toward the target value
            energyAmount = Mathf.MoveTowards(energyAmount, target, energyChangeSpeed * Time.deltaTime);
            emfAmount = Mathf.MoveTowards(emfAmount, emfTarget, energyChangeSpeed * Time.deltaTime);
        }
    }


    private void MoveEnergyAtConstantSpeed()
    {
        Vector3 targetWorldPos = transform.position + currentTargetOffset;
        Vector3 currentPos = energyGameObj.transform.position;

        // Move toward the target at constant speed
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
