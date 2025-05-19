using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CandleTask : NetworkBehaviour
{
    [Header("References")]
    public GameObject candlePrefab;
    public GameObject candleContainer;
    public GameObject candleItemPrefab;
    public GameObject newCandle;

    [Header("Task settings")]
    public List<CandleCircle> noOfCandlesInCircles = new();
    public bool gameStarted;
    public List<Candle__Task> allCandles = new();
    public List<Candle__Task> activationCode = new();
    public int noOfIteration = 15;
    public int iterationDone = 0;

    [Header("Each One")]
    public int currentIteration = 0;
    public float currentTimeToWait = 1;


    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnCandle;
    }

    private void SpawnCandle()
    {
        for (int n = 0; n < noOfCandlesInCircles.Count; n++)
        {

            for (int i = 0; i < noOfCandlesInCircles[n].noOfCandles; i++)
            {
                double y = noOfCandlesInCircles[n].disttanceFromCenter * (Mathf.Sin(Mathf.PI * 2 * i / noOfCandlesInCircles[n].noOfCandles));
                double x = noOfCandlesInCircles[n].disttanceFromCenter * (Mathf.Cos(Mathf.PI * 2 * i / noOfCandlesInCircles[n].noOfCandles));
                Vector3 pos = new Vector3((float)x, candlePrefab.transform.position.y, (float)y);

                GameObject candle = Instantiate(candlePrefab, pos, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
                NetworkObject obj = candle.GetComponent<NetworkObject>();
                obj.Spawn();
                obj.TrySetParent(candleContainer.transform, false);
            }
        }

        for (int i = 0; i < candleContainer.transform.childCount; i++)
        {
            allCandles.Add(candleContainer.transform.GetChild(i).GetComponent<Candle__Task>());
        }
    }


    public void StartGame()
    {
        Debug.LogWarning("hjk");
        gameStarted = true;
        StartCoroutine(StartIteration());
    }

    IEnumerator StartIteration()
    {
        while (currentIteration < noOfIteration)
        {
            currentIteration++;
            yield return new WaitForSeconds(currentTimeToWait);
            AddCandle(allCandles[Random.Range(0, allCandles.Count)]);
        }
    }

    private void AddCandle(Candle__Task candle__Task)
    {
        if (!candle__Task.isLit.Value)
        {
            AddCandle(allCandles[Random.Range(0, allCandles.Count)]);
            return;
        }
        if (activationCode.Count > 0)
            activationCode[activationCode.Count - 1].SetNormalMat();
        activationCode.Add(candle__Task);
        candle__Task.isLit.Value = false;
        candle__Task.fire.Stop();
        candle__Task.G1.material = candle__Task.unlittingMat;
        candle__Task.G2.material = candle__Task.unlittingMat;
        if (activationCode.Count > 0)
        {
            activationCode[0].active.Value = true;
        }
    }

    public void Correct()
    {
        activationCode.RemoveAt(0);
        iterationDone++;
        if (activationCode.Count > 0)
        {
            activationCode[0].active.Value = true;
        }
        else if (currentIteration >= noOfIteration && iterationDone >= noOfIteration)
        {
            gameStarted = false;
            newCandle = Instantiate(candleItemPrefab, transform.position + new Vector3(0, 5, 0),  transform.rotation);
            NetworkObject obj = newCandle.GetComponent<NetworkObject>();
            obj.Spawn();
            activationCode.Clear();
            currentIteration = 0;
            gameStarted = false;
        }
    }

    internal void InCorrect()
    {
        StopAllCoroutines();
        activationCode.Clear();
        currentIteration = 0;
        gameStarted = false;
    }
}

[System.Serializable]
public class CandleCircle
{
    public int noOfCandles;
    public float disttanceFromCenter;
}