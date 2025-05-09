using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PurePowderTask : MonoBehaviour
{
    [Header("Total Iteration Settings")]
    [SerializeField] int totalIteration = 3;
    [SerializeField] int iterationLeft = 3;
    [SerializeField] int currentIteration = 1;

    [Header("One Iteration Settings")]
    [SerializeField] List<Material> colorCodes = new();
    [SerializeField] List<Material> currentColourCode = new();
    [SerializeField] bool gameSatrted;
    [SerializeField] int colourToShow;
    [SerializeField] int currentColour = 0;
    [SerializeField] float timeForShowing1Color = 1;
    [SerializeField] List<Material> totalColorMaterials;
    [SerializeField] public Material neutralColourMaterial;
    [SerializeField] Material correctAnsMaterial;
    [SerializeField] Material wrongAnsMaterial;
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
        if (!gameSatrted && animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
        {
            Debug.LogWarning("Game Started!");
            gameSatrted = true;
            iterationLeft = totalIteration;
            StartIteration();
        }
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
            }

            if (colourToShow > 0)
            {
                waitFor1Sec = true;
                timeUp = false;
                totalTime = timeForShowing1Color;
                currentColour++;
            }
            else if (colourToShow == 0)
            {
                ScreenColour.material = neutralColourMaterial;
                showColour = false;
            }


            colourToShow--;
        }
    }

    private void StartIteration()
    {
        iterationLeft--;
        Material newColor = totalColorMaterials[Random.Range(0, totalColorMaterials.Count)];
        colorCodes.Add(newColor);

        colourToShow = currentIteration;
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
            Correct();
        }
    }

    private void Correct()
    {
        if (iterationLeft > 0)
        {
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
            gameSatrted = false;
            currentColour = 0;
            time = 0;
            timeUp = true;
            ShowCorrectVisual();
        }
    }

    private void ShowCorrectVisual()
    {
        animator.SetTrigger("correct");
    }

    private void Wrong()
    {
        iterationLeft = totalIteration;
        currentIteration = 1;
        colorCodes.Clear();
        currentColourCode.Clear();
        gameSatrted = false;
        currentColour = 0;
        time = 0;
        timeUp = true;

        ShowWrongVisual();
    }

    private void ShowWrongVisual()
    {
        animator.SetTrigger("wrong");
    }
}
