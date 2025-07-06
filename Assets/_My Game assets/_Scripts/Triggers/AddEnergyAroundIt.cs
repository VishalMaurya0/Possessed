using UnityEngine;

public class AddEnergyAroundIt : MonoBehaviour
{
    public GameObject energyPrefab;
    public float roamRange = 5f;
    public float energyRange = 3f;
    public float maxEnergy = 50f;
    public float roamSpeed = 2f;
    public bool isActive;

    private EnergyTrigger et;
    private GameObject energyGameObj;
    private Vector3 currentTargetOffset;

    private void Start()
    {
        energyGameObj = Instantiate(energyPrefab);
        et = energyGameObj.AddComponent<EnergyTrigger>();
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
            energyGameObj.GetComponent<SphereCollider>().radius = energyRange;
            PickNewTarget();
        }
    }

    private void Update()
    {
        if (energyGameObj != null)
        {
            MoveEnergyAtConstantSpeed();
            et.isActive = isActive;
            et.EnergyAmount = maxEnergy;
            et.energyRange = energyRange;
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
