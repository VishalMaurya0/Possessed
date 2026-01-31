using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PurePowderTask : NetworkBehaviour
{
    [Header("Total Iteration Settings")]
    [SerializeField] int totalIteration = 3;
    [SerializeField] int iterationLeft = 3;
    [SerializeField] int currentIteration = 1;

    [Header("One Iteration Settings")]
    [SerializeField] List<Material> colorCodes = new();
    [SerializeField] List<Material> currentColourCode = new();
    [SerializeField] NetworkVariable <bool> gameSatrted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] int noOfColoursToShow;
    [SerializeField] int currentColour = 0;
    [SerializeField] float timeForShowing1Color = 1;
    [SerializeField] List<Material> totalColorMaterials;
    [SerializeField] public Material neutralColourMaterial;
    [SerializeField] Material correctAnsMaterial;
    [SerializeField] Material wrongAnsMaterial;
    [SerializeField] NetworkVariable <int> currentScreenColour = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] MeshRenderer ScreenColour;

    [Header("Task Settings & References")]
    [SerializeField] GameObject PurePowderPrefab;
    [SerializeField] GameObject PurePowder;
    Animator animator;

    [Header("Code Settings")]
    public float totalTime;
    public float time = 0;
    public bool waitFor1Sec;
    public bool timeUp = true;
    public bool showColour;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnMouseUp()
    {
        if (!gameSatrted.Value && animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
        {
            StartGameServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc()
    {
        gameSatrted.Value = true;
        iterationLeft = totalIteration;
        StartIteration();
    }

    private void Update()
    {
        if (waitFor1Sec)
        {
            time += Time.deltaTime;
            if (time > totalTime)
            {
                time = 0;
                waitFor1Sec = false;
                timeUp = true;
            }
        }

        if (showColour && timeUp)
        {
            if (currentColour < colorCodes.Count)
            {
                ScreenColour.material = colorCodes[currentColour];

                //currentScreenColour.Value = FindIndexOfColor(colorCodes[currentColour]);
                AudioManager.PlaySound(AudioType.SmallPoup, null, 1, 0.7f);
                //ChangeColourClientRpc();
            }

            if (noOfColoursToShow > 0)
            {
                waitFor1Sec = true;
                timeUp = false;
                totalTime = timeForShowing1Color;
                currentColour++;
            }
            else if (noOfColoursToShow == 0)
            {
                //currentScreenColour.Value = neutralColourMaterial;
                ScreenColour.material = neutralColourMaterial;
                //ChangeColourClientRpc();
                showColour = false;
            }


            noOfColoursToShow--;
        }
    }

    //[ClientRpc]
    //private void ChangeColourClientRpc()
    //{
    //    ScreenColour.material = totalColorMaterials[currentScreenColour.Value];
    //}

    private void StartIteration() //runs on server
    {
        int code;
        if (IsServer)
        {
            code = Random.Range(0, totalColorMaterials.Count);
            StartIterationClientRpc(code);
        }
        else
        {
            return;
        }

        Material newColor = totalColorMaterials[code];
        colorCodes.Add(newColor);
        iterationLeft--;

        noOfColoursToShow = currentIteration;
        currentColour = 0;
        showColour = true;
        currentIteration++;

        currentColourCode.Clear(); // Important for fresh input
    }

    [ClientRpc]
    private void StartIterationClientRpc(int code)
    {
        if (IsServer) { return; }


        iterationLeft--;
        Material newColor = totalColorMaterials[code];
        colorCodes.Add(newColor);

        noOfColoursToShow = currentIteration;
        currentColour = 0;
        showColour = true;
        currentIteration++;

        currentColourCode.Clear(); // Important for fresh input
    }

    public void AddColour(Material colour)
    {
        currentColourCode.Add(colour);
        CheckAns();
    }

    private void CheckAns()
    {
        for (int i = 0; i < currentColourCode.Count; i++)
        {
            if (i >= colorCodes.Count || currentColourCode[i] != colorCodes[i])
            {
                Wrong();
                return;
            }
        }

        if (currentColourCode.Count == currentIteration - 1)
        {
            StartCoroutine(Correct());
        }
    }

    private IEnumerator Correct()
    {
        if (iterationLeft > 0)
        {
            yield return new WaitForSeconds(1);
            StartIteration();
        }
        else
        {
            PurePowder = Instantiate(PurePowderPrefab, transform.parent.position + new Vector3(0, 3, 5), transform.parent.rotation);
            PurePowder.GetComponent<NetworkObject>().Spawn();
            iterationLeft = totalIteration;
            currentIteration = 1;
            colorCodes.Clear();
            currentColourCode.Clear();
            gameSatrted.Value = false;
            currentColour = 0;
            time = 0;
            timeUp = true;
            ShowCorrectVisual();
        }
    }

    private void ShowCorrectVisual()
    {
        AudioManager.PlaySound(AudioType.Correct);
        animator.SetTrigger("correct");
    }

    private void Wrong()
    {
        iterationLeft = totalIteration;
        currentIteration = 1;
        colorCodes.Clear();
        currentColourCode.Clear();
        gameSatrted.Value = false;
        currentColour = 0;
        time = 0;
        timeUp = true;

        ShowWrongVisual();
    }

    private void ShowWrongVisual()
    {
        animator.SetTrigger("wrong");
        AudioManager.PlaySound(AudioType.Wrong);
    }


    int FindIndexOfColor(Material mate)
    {
        return currentScreenColour.Value =
    totalColorMaterials.FindIndex(
        mat => mat.color == mate.color
    );

    }
}
