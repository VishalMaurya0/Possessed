using Unity.Netcode;
using UnityEngine;

public class CupForCoinTask : NetworkBehaviour
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
    public NetworkVariable <bool> clickable = new NetworkVariable<bool> (true , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable <bool> getCoin = new NetworkVariable<bool> (false , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable <bool> containsCoin = new NetworkVariable<bool> (false , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Time Settings For Starting The game")]
    [SerializeField]float startTime = 1;
    [SerializeField] float timeForClickedState = 0.02f;
    [SerializeField] NetworkVariable <float> time = new NetworkVariable<float> (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable <bool> start = new NetworkVariable<bool> (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]bool showCup = false;
    bool allInputDisabled____onServerVar = false;
    NetworkVariable <bool> startAnim = new NetworkVariable<bool> (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


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
        if (start.Value && IsServer)
        {
            time.Value += Time.deltaTime;

            // start Animations
            if (time.Value > 0 && time.Value < timeForClickedState)
                startAnim.Value = true;

            // End Animations
            if (time.Value > timeForClickedState + 0.1f)   
                startAnim.Value = false;
            

            // = start game = //
            if (time.Value > startTime)
            {
                time.Value = 0;
                start.Value = false;
                taskForCoins.restart = true;
            }
        }

        if (startAnim.Value)
        {
            Animations();
        }


        if (showCup)
        {
            time.Value += Time.deltaTime;

            float curveTime = (time.Value % showingTime) / showingTime; // loops
            offsetY = yCurve.Evaluate(curveTime);
            transform.localPosition = new Vector3(startPos.x, startPos.y + offsetY, startPos.z);

            if (time.Value >= showingTime)
            {
                showCup = false;
                time.Value = 0;

                taskForCoins.cupForCoinTasks.ForEach(task => { task.clickable.Value = true; });
                taskForCoins.cupForCoinTasks.ForEach(task => { task.getCoin.Value = false; });
            }

            if (time.Value >= showingTime / 2 && containsCoin.Value)
            {
                time.Value = showingTime / 2;
            }

            if (containsCoin.Value && newCoin == null)
            {
                containsCoin.Value = false;
            }
        }
    }

    private void Animations()
    {
        Material[] mats = renderar.materials;
        if (time.Value < timeForClickedState)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = clickedMat;
            }
            renderar.materials = mats;
        }else if (time.Value >= timeForClickedState && time.Value < timeForClickedState + 0.1f)   //------- for this time the mat changes to normal otherwise its permanent becomes gold and give control after 0.01s
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
        MouseClickedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MouseClickedServerRpc()
    {
        if (allInputDisabled____onServerVar)
        {
            return;
        }
        allInputDisabled____onServerVar = true;

        if (!clickable.Value)
        {
            foreach (var cup in taskForCoins.cupForCoinTasks)
            {
                cup.clickable.Value = true;
                cup.containsCoin.Value = false;
                cup.getCoin.Value = false;
                cup.showCup = false;
                cup.start.Value = false;
                cup.time.Value = 0;
                if (cup.newCoin != null)
                {
                    cup.newCoin.GetComponent<NetworkObject>().Despawn();
                    Destroy(cup.newCoin);
                }
                startAnim.Value = false;
                taskForCoins.positionReset = true;
                Material[] mats = renderar.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = normalMat;
                }
                renderar.materials = mats;
            }
            allInputDisabled____onServerVar = false;
            return;
        }


        if (clickable.Value && !getCoin.Value)
        {
            taskForCoins.cupForCoinTasks.ForEach(task => { task.clickable.Value = false; });
            start.Value = true;
            containsCoin.Value = true;
            allInputDisabled____onServerVar = false;
            return;
        }

        if (clickable.Value && getCoin.Value)
        {
            taskForCoins.cupForCoinTasks.ForEach(task => { task.clickable.Value = false; });
            showCup = true;
            startPos = transform.localPosition;

            if (containsCoin.Value)
            {
                taskForCoins.IncreaseDifficulty();
                newCoin = Instantiate(coinPrefab, transform.position, transform.rotation, transform);
                newCoin.GetComponent<NetworkObject>().Spawn();
                newCoin.GetComponent<Rigidbody>().useGravity = false;
            }
            allInputDisabled____onServerVar = false;
            return;
        }

        allInputDisabled____onServerVar = false;
    }
}
