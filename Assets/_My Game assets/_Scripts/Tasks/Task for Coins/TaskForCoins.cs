using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.Progress;

public class TaskForCoins : MonoBehaviour
{
    [Header("Task Settings")]
    [SerializeField] int startingIterationDifficulty = 6;
    [SerializeField] int startingGlassNoDifficulty = 1;
    bool gameStarted = false;

    [Header("Each Iteration Settings")]
    [SerializeField] int currentIterationDifficulty = 6;
    [SerializeField] int currentGlassNoDifficulty = 1;
    [SerializeField] List<Glass> movingGlasses = new();
    [SerializeField] List<Glass> savedMovingGlasses = new();
    [SerializeField] List<Place> nextPlaces = new();
    [SerializeField] List<Place> savedPlaces = new();
    [SerializeField] GameObject glassContainer;
    [SerializeField] List<Glass> glasses = new();
    [SerializeField] List<Place> places = new();
    [SerializeField] int iterationLeft;
    bool hasStartedNextIteration = false;

    private void Start()
    {
        Debug.LogWarning("Initializing Glasses and Places...");
        glasses.Clear();
        for (int i = 0; i < glassContainer.transform.childCount; i++)
        {
            Glass glass = new Glass(glassContainer.transform.GetChild(i).gameObject, i);
            glasses.Add(glass);
            places.Add(new Place(i, glasses[i]));
        }
        Debug.LogWarning($"Initialized {glasses.Count} glasses and places.");


        if (!gameStarted)
        {
            Debug.LogWarning("Game Started!");
            gameStarted = true;
            currentIterationDifficulty = startingIterationDifficulty;
            iterationLeft = currentIterationDifficulty;
            currentGlassNoDifficulty = startingGlassNoDifficulty;
            Debug.LogWarning($"Starting first iteration with {currentGlassNoDifficulty} glasses to move.");
            StartIteration();
        }
    }

    private void Update()
    {
        for (int i = 0; i < glasses.Count; i++)
        {
            Move(i);
        }
    }

    private void OnMouseUp()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            currentIterationDifficulty = startingIterationDifficulty;
            iterationLeft = currentIterationDifficulty;
            currentGlassNoDifficulty = startingGlassNoDifficulty;
            StartIteration();
        }
    }

    private void StartIteration()
    {
        if (iterationLeft <= 0)
        {
            Debug.LogWarning("No more iterations left.");
            return;
        }

        iterationLeft--;

        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            FillMovingGlassesAtStart(0);
        }
        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            SelectPlaceToGoAtStart(0, i);
            movingGlasses[i].currentID = nextPlaces[i].ID;
            nextPlaces[i].comingGlass = movingGlasses[i];
            Debug.LogWarning($"Glass {i} will move to place {nextPlaces[i].ID}");
        }

        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            movingGlasses[i].nextPosition = places[i].myPosition;
            movingGlasses[i].moving = true;
        }

    }

    private void FillMovingGlassesAtStart(int iter)
    {
        if (nextPlaces.Count > 0)
        {
            int ID = nextPlaces[0].ID;
            if (places[ID].filled && !places[ID].currentGlass.moving)
            {
                movingGlasses.Add(places[ID].currentGlass);
                places[ID].filled = false;
                nextPlaces.RemoveAt(0);
                return;
            }
            nextPlaces.RemoveAt(0);
        }

        int randomID = UnityEngine.Random.Range(0, places.Count);
        if (places[randomID].filled && !places[randomID].currentGlass.moving)
        {
            movingGlasses.Add(places[randomID].currentGlass);
            places[randomID].filled = false;
        }
        else if (iter < 10000)
        {
            FillMovingGlassesAtStart(iter + 1);
        }
        else
        {
            Debug.LogError("Recursion End in FillMovingGlassesAtStart");
        }
    }

    private void SelectPlaceToGoAtStart(int iter, int i)
    {
        int randomID = UnityEngine.Random.Range(0, places.Count);
        if (!places[randomID].toBeFilled && movingGlasses[i].currentID != randomID)
        {
            Debug.LogWarning($"Selected Glass ID: {movingGlasses[i].currentID} Selected Place ID {randomID}");
            nextPlaces.Add(places[randomID]);
            places[randomID].toBeFilled = true;
        }
        else if (iter < 1000)
        {
            SelectPlaceToGoAtStart(iter + 1, i);
        }
        else
        {
            Debug.LogError("Recursion End in SelectPlaceToGoAtStart");
        }
    }

    private void Move(int i)
    {
        Glass glass = glasses[i];
        if (glass.moving && glass.time < 1.8f)
        {
            float t = glass.time;
            if (glass.time > 1)
                t = 1;
            glass.time += Time.deltaTime;
            Vector3 change = glass.nextPosition - glass.previousPosition;
            float sign = change.x / change.magnitude * (-1);
            glass.currentPosition = (glass.previousPosition + glass.nextPosition) / 2 +
                new Vector3(change.magnitude / 2 * sign * MathF.Cos(t * MathF.PI), 0, change.magnitude / 2 * MathF.Sin(t * MathF.PI));

            if (glass.time > 0.7 && !hasStartedNextIteration && !glass.RunOnce)
            {
                for (int j = 0; j < movingGlasses.Count; j++)
                {
                    movingGlasses[j].RunOnce = true;
                }
                savedPlaces.Clear();
                hasStartedNextIteration = true;
                for (int j = 0; j < currentGlassNoDifficulty; j++)
                {
                    savedPlaces.Add(nextPlaces[j]);
                    nextPlaces[j].toBeFilled = false;
                }
                savedMovingGlasses.Clear();
                for (int j = 0; j < movingGlasses.Count; j++)
                {
                    savedMovingGlasses.Add(movingGlasses[j]);
                }
                movingGlasses.Clear();
                StartIteration();
            }

            if (glass.time > 1.1f)
            {
                for (int j = 0; j < savedMovingGlasses.Count; j++)
                {
                    savedMovingGlasses[j].time = 0;
                    savedMovingGlasses[j].moving = false;
                    savedMovingGlasses[j].previousPosition = savedMovingGlasses[j].currentPosition;
                }
                for (int j = 0; j < currentGlassNoDifficulty; j++)
                {
                    savedPlaces[j].filled = true;
                    savedPlaces[j].currentGlass = savedPlaces[j].comingGlass;
                }
                hasStartedNextIteration = false;
            }
        }else if (!glass.moving)
        {
            glass.RunOnce = false;
        }
        glasses[i].glassG.transform.localPosition = glasses[i].currentPosition;
    }
}

[System.Serializable]
public class Glass
{
    public GameObject glassG;
    public int previousID;
    public int currentID;
    public int nextID;
    public Vector3 previousPosition;
    public Vector3 nextPosition;
    public Vector3 currentPosition;
    public bool moving;
    public bool RunOnce;
    public float time;

    public Glass(GameObject glass, int id)
    {
        glassG = glass;
        previousPosition = glassG.transform.localPosition;
        currentPosition = glassG.transform.localPosition;
        previousID = id;
        currentID = id;
    }

    public Glass(Glass glass)
    {
        glassG = glass.glassG;
        previousID = glass.previousID;
        currentID = glass.currentID;
        previousPosition = glass.previousPosition;
        currentPosition = glass.currentPosition;
        nextPosition = glass.nextPosition;
        moving = glass.moving;
        RunOnce = glass.RunOnce;
        time = glass.time;
    }
}

[Serializable]
public class Place
{
    public int ID;
    public bool filled;
    public bool toBeFilled;
    public Vector3 myPosition;
    public Glass currentGlass;
    public Glass comingGlass;

    public Place(int ID, Glass glass)
    {
        this.ID = ID;
        this.filled = true;
        this.toBeFilled = false;
        myPosition = glass.currentPosition;
        currentGlass = glass;
    }

    public Place(Place place)
    {
        ID = place.ID;
        filled = place.filled;
        this.toBeFilled = place.toBeFilled;
        myPosition = place.myPosition;
        currentGlass = new Glass(currentGlass);
        comingGlass = new Glass(comingGlass);
    }
}
