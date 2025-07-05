using UnityEngine;

public class AddEnergyAroundIt : MonoBehaviour
{
    public GameObject energyPrefab;
    public float roamRange;
    public float energyRange;
    public float maxEnergy;
    public bool isActive;
    EnergyTrigger et;
    private GameObject energyGameObj; 
    private Vector3 currentTargetOffset;
    private float roamChangeInterval = 7f;
    private float nextChangeTime;

    private void Start()
    {
        GameManager.onServerStarted += GameManager_onServerStarted;
    }

    private void GameManager_onServerStarted()
    {
        energyGameObj = Instantiate(energyPrefab);
        et = energyGameObj.AddComponent<EnergyTrigger>();
        energyGameObj.GetComponent<SphereCollider>().radius = energyRange;
    }

    private void Update()
    {
        if (energyGameObj != null)
        {
            energyGameObj.transform.position = transform.position + RoamPos();
            et.isActive = isActive;
            et.EnergyAmount = maxEnergy;
            et.energyRange = energyRange;
        }
    }

    private Vector3 RoamPos()
    {
        if (Time.time > nextChangeTime)
        {
            currentTargetOffset = new Vector3(
                UnityEngine.Random.Range(-roamRange, roamRange),
                0f,
                UnityEngine.Random.Range(-roamRange, roamRange)
            );
            nextChangeTime = Time.time + roamChangeInterval;
        }

        return Vector3.Lerp(energyGameObj.transform.position - transform.position, currentTargetOffset, Time.deltaTime);   //Champt GPT
    }

}

public class EnergyTrigger : MonoBehaviour
{
    public float EnergyAmount;
    public bool isActive;
    public float energyRange;
}