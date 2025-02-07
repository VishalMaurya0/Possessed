using UnityEngine;

public class SafePointArea : MonoBehaviour
{
    public float safePointTimerDuration;
    public bool active;

    public float safepointTimer;
    public int noOfPlayers;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FearMeter fearMeter = other.gameObject.GetComponent<FearMeter>();
            fearMeter.SAFE = true;
            noOfPlayers++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FearMeter fearMeter = other.gameObject.GetComponent<FearMeter>();
            fearMeter.SAFE = false;
            noOfPlayers--;
        }
    }

    private void Update()
    {
        HandleActivation();
    }

    private void HandleActivation()
    {
        if (safepointTimer >= safePointTimerDuration)
        {
            active = true;
        }


        if (noOfPlayers > 0 && active)
        {
            safepointTimer -= Time.deltaTime * noOfPlayers;
            if (safepointTimer <= 0)
            {
                active = false;
            }
        }

        if (!active)
        {
            safepointTimer += Time.deltaTime;
        }

        if (noOfPlayers <= 0 && active && safepointTimer < safePointTimerDuration)
        {
            safepointTimer += Time.deltaTime * 1.5f;
        }
    }
}
