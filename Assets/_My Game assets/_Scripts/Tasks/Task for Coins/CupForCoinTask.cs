using System;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

public class CupForCoinTask : MonoBehaviour
{
    [Header("References")]
    public TaskForCoins taskForCoins;
    Animator animator;
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject newCoin;


    [Header("Cup Settings")]
    [SerializeField] Material glowMat;
    [SerializeField] Material clickedMat;
    [SerializeField] Material normalMat;
    [SerializeField] MeshRenderer renderar;
    public bool clickable = true;
    public bool getCoin = false;
    public bool containsCoin = false;

    [Header("Time Settings For Starting The game")]
    [SerializeField]float startTime = 1;
    [SerializeField] float timeForClickedState = 0.02f;
    [SerializeField]float time = 0;
    [SerializeField]bool start = false;
    [SerializeField]bool showCup = false;


    public AnimationCurve yCurve;
    public float showingTime = 1f;
    private Vector3 startPos;
    float offsetY;

    private void Start()
    {
        renderar = GetComponent<MeshRenderer>();
        taskForCoins = GetComponentInParent<TaskForCoins>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (start)
        {
            time += Time.deltaTime;

            // start Animations
            Animations();

            // = start game = //
            if (time > startTime)
            {
                time = 0;
                start = false;
                taskForCoins.restart = true;
            }
        }


        if (showCup)
        {
            time += Time.deltaTime;

            float curveTime = (time % showingTime) / showingTime; // loops
            offsetY = yCurve.Evaluate(curveTime);
            transform.localPosition = new Vector3(startPos.x, startPos.y + offsetY, startPos.z);

            if (time >= showingTime)
            {
                showCup = false;
                time = 0;

                taskForCoins.cupForCoinTasks.ForEach(task => { task.clickable = true; });
                taskForCoins.cupForCoinTasks.ForEach(task => { task.getCoin = false; });
            }

            if (time >= showingTime / 2 && containsCoin)
            {
                time = showingTime / 2;
            }

            if (containsCoin && newCoin == null)
            {
                containsCoin = false;
            }
        }
    }

    private void Animations()
    {
        Material[] mats = renderar.materials;
        if (time < timeForClickedState)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = clickedMat;
            }
            renderar.materials = mats;
        }else if (time >= timeForClickedState && time < timeForClickedState + 0.01f)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = normalMat;
            }
            renderar.materials = mats;
        }

    }

    private void OnMouseEnter()
    {
        Material[] mats = renderar.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = glowMat;
        }
        renderar.materials = mats;
    }

    private void OnMouseExit()
    {
        Material[] mats = renderar.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = normalMat;
        }
        renderar.materials = mats;
    }

    private void OnMouseUp()
    {
        if (!clickable)
        {
            foreach (var cup in taskForCoins.cupForCoinTasks)
            {
                cup.clickable = true;
                cup.containsCoin = false;
                cup.getCoin = false;
                cup.showCup = false;
                cup.start = false;
                cup.time = 0;
                if (cup.newCoin != null)
                {
                    cup.newCoin.GetComponent<NetworkObject>().Despawn();
                    Destroy(cup.newCoin);
                }
                taskForCoins.positionReset = true;
            }
            return;
        }
     
        
        if (clickable && !getCoin)
        {
            taskForCoins.cupForCoinTasks.ForEach(task => { task.clickable = false; });
            start = true;
            containsCoin = true;
            return;
        }
        
        if (clickable && getCoin)
        {
            taskForCoins.cupForCoinTasks.ForEach(task => { task.clickable = false; });
            showCup = true;
            startPos = transform.localPosition;

            if (containsCoin)
            {
                taskForCoins.IncreaseDifficulty();
                newCoin = Instantiate(coinPrefab, transform.position, transform.rotation, transform);
                newCoin.GetComponent<NetworkObject>().Spawn();
                newCoin.GetComponent<Rigidbody>().useGravity = false;
            }
            return;
        }

    }
}
