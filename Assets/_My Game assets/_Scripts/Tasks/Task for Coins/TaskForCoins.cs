using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TaskForCoins : MonoBehaviour
{
    [Header("Task Settings")]
    [SerializeField] int startingIterationDifficulty = 6;
    [SerializeField] int startingGlassNoDifficulty = 1;
    bool gameStarted = false;
    bool gameEnd = false;

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
        Debug.LogError("Start: Initializing Glasses and Places...");
        glasses.Clear();
        for (int i = 0; i < glassContainer.transform.childCount; i++)
        {
            Glass glass = new Glass(glassContainer.transform.GetChild(i).gameObject, i);
            glasses.Add(glass);
            places.Add(new Place(i, glasses[i]));
        }
        Debug.LogError($"Start: Initialized {glasses.Count} glasses and places.");

        if (!gameStarted)
        {
            Debug.LogError("Start: Game Started!");
            gameStarted = true;
            currentIterationDifficulty = startingIterationDifficulty;
            iterationLeft = currentIterationDifficulty;
            currentGlassNoDifficulty = startingGlassNoDifficulty;
            Debug.LogError($"Start: Starting first iteration with {currentGlassNoDifficulty} glasses to move.");
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
            Debug.LogError("OnMouseUp: Game manually started via mouse.");
            gameStarted = true;
            currentIterationDifficulty = startingIterationDifficulty;
            iterationLeft = currentIterationDifficulty;
            currentGlassNoDifficulty = startingGlassNoDifficulty;
            StartIteration();
        }
    }

    private void StartIteration()
    {
        if (iterationLeft == 0)
        {
            iterationLeft--;
            LastIteration();
            return;
        }
        else if (iterationLeft <= 0)
        {
            Debug.LogError("StartIteration: No more iterations left.");
            return;
        }
        
        iterationLeft--;
        
        Debug.LogError($"StartIteration: Iteration {startingIterationDifficulty - iterationLeft}/{startingIterationDifficulty}");

        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            FillMovingGlassesAtStart(0);
        }

        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            SelectPlaceToGoAtStart(0, i);
            movingGlasses[i].currentID = nextPlaces[i].ID;
            nextPlaces[i].comingGlass = movingGlasses[i];
            if (savedMovingGlasses.Count > 0)
            {
                movingGlasses[i].dirOfRoation = (-1) * savedMovingGlasses[i].dirOfRoation;
            }
        }

        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            movingGlasses[i].moving = true;
        }
    }

    private void LastIteration()
    {
        gameEnd = true;
        Debug.LogError($"[LastIteration] Moving Glasses Count: {movingGlasses.Count}");
        while (nextPlaces.Count > 0)
        {
            int ID = nextPlaces[0].ID;
            Debug.LogError($"[LastIteration] Processing ID from nextPlaces: {ID}");

            if (ID < 0 || ID >= places.Count)
            {
                Debug.LogError($"[LastIteration] ERROR: Invalid ID {ID} - out of bounds of places list.");
                return;
            }

            if (places[ID].filled)
            {
                if (places[ID].currentGlass == null)
                {
                    Debug.LogError($"[LastIteration] ERROR: Place {ID} is marked filled, but currentGlass is null.");
                }
                else
                {
                    Debug.LogError($"[LastIteration] Adding currentGlass from place {ID} to movingGlasses.");
                    movingGlasses.Add(places[ID].currentGlass);
                }
            }
            nextPlaces.RemoveAt(0);
            Debug.LogError($"[LastIteration] Removed processed place from nextPlaces. Remaining: {nextPlaces.Count}");
        }

        for (int i = 0; i < places.Count; i++)
        {
            if (places[i].currentGlass.glassG == null && places[i].comingGlass.glassG == null)
            {
                Debug.LogError($"[LastIteration] Place {i} is empty. Adding to nextPlaces.");
                nextPlaces.Add(places[i]);
            }
        }


        if (movingGlasses.Count > nextPlaces.Count)
        {
            Debug.LogError($"[LastIteration] ERROR: Not enough empty places to move all glasses. movingGlasses: {movingGlasses.Count}, nextPlaces: {nextPlaces.Count}");
            return;
        }

        for (int i = 0; i < movingGlasses.Count; i++)
        {
            Debug.LogError($"[LastIteration] Moving glass {i} to place ID {nextPlaces[i].ID}");
            movingGlasses[i].currentID = nextPlaces[i].ID;
            nextPlaces[i].comingGlass = movingGlasses[i];
            movingGlasses[i].nextPosition = nextPlaces[i].myPosition;
        }

        for (int i = 0; i < movingGlasses.Count; i++)
        {
            Debug.LogError($"[LastIteration] Setting glass {i} to moving.");
            movingGlasses[i].moving = true;
        }
    }


    private void FillMovingGlassesAtStart(int iter)
    {
        Debug.LogWarning($"FillMovingGlassesAtStart: Iteration {iter} started");

        if (nextPlaces.Count > 0)
        {
            int ID = nextPlaces[0].ID;
            Debug.LogWarning($"FillMovingGlassesAtStart: Checking saved nextPlace ID {ID}");

            if (places[ID].filled && !places[ID].currentGlass.moving)
            {
                movingGlasses.Add(places[ID].currentGlass);
                places[ID].filled = false;
                places[ID].currentGlass = null;
                Debug.LogError($"FillMovingGlassesAtStart: Picked from saved nextPlace ID {ID}");
                nextPlaces.RemoveAt(0);
                return;
            }
            else
            {
                Debug.LogWarning($"FillMovingGlassesAtStart: Skipped ID {ID} - either not filled or already moving");
            }

            nextPlaces.RemoveAt(0);
        }

        int randomID = UnityEngine.Random.Range(0, places.Count);
        Debug.LogWarning($"FillMovingGlassesAtStart: Trying randomID {randomID}");

        if (places[randomID].filled && !places[randomID].currentGlass.moving && !movingGlasses.Contains(places[randomID].currentGlass))
        {
            movingGlasses.Add(places[randomID].currentGlass);
            Debug.LogWarning($"FillMovingGlassesAtStart: Added glass from randomID {randomID} to movingGlasses");

            if ((places[randomID].comingGlass != null && !places[randomID].comingGlass.moving) || (places[randomID].comingGlass == null))
            {
                places[randomID].filled = false;
                places[randomID].currentGlass = null;
                Debug.LogError($"FillMovingGlassesAtStart: Picked random place {randomID}");
            }
            else
            {
                Debug.LogWarning($"FillMovingGlassesAtStart: Did not unfill randomID {randomID} due to moving comingGlass");
            }
        }
        else if (iter < 400)
        {
            Debug.LogWarning($"FillMovingGlassesAtStart: Recursing, attempt {iter + 1}");
            FillMovingGlassesAtStart(iter + 1);
        }
        else
        {
            Debug.LogError("FillMovingGlassesAtStart: Recursion limit reached!");
        }
    }


    private void SelectPlaceToGoAtStart(int iter, int i)
    {
        int randomID = UnityEngine.Random.Range(0, places.Count);
        if (!places[randomID].toBeFilled && movingGlasses[i].currentID != randomID)
        {
            Debug.LogError($"SelectPlaceToGoAtStart: Glass {i} moving from {movingGlasses[i].currentID} to {randomID}");
            nextPlaces.Add(places[randomID]);
            movingGlasses[i].nextPosition = places[randomID].myPosition;
            places[randomID].toBeFilled = true;
        }
        else if (iter < 400)
        {
            SelectPlaceToGoAtStart(iter + 1, i);
        }
        else
        {
            Debug.LogError("SelectPlaceToGoAtStart: Recursion limit reached!");
        }
    }


    private void Move(int i)
    {
        Glass glass = glasses[i];

        if (glass.moving && glass.time < 1.8f)
        {
            float t = glass.time > 1 ? 1 : glass.time;
            glass.time += Time.deltaTime;

            Vector3 change = glass.nextPosition - glass.previousPosition;
            float sign = change.x / change.magnitude * (-1);

            glass.currentPosition = (glass.previousPosition + glass.nextPosition) / 2 +
                new Vector3(
                change.magnitude / 2 * sign * MathF.Cos(t * MathF.PI), 
                0, 
                change.magnitude / 2 * glass.dirOfRoation * MathF.Sin(t * MathF.PI)
                );

            if (glass.time > 0.7f && !hasStartedNextIteration && !glass.hasRunCodeOnce)
            {
                Debug.LogError($"Move: Glass {i} reached 0.7f time, triggering next iteration prep.");

                for (int j = 0; j < movingGlasses.Count; j++)
                {
                    movingGlasses[j].hasRunCodeOnce = true;
                }

                savedPlaces.Clear();
                hasStartedNextIteration = true;

                for (int j = 0; j < currentGlassNoDifficulty; j++)
                {
                    savedPlaces.Add(nextPlaces[j]);
                    Debug.LogError($"Move: Saved Place {nextPlaces[j].ID} for glass {nextPlaces[j].ID}");
                    nextPlaces[j].toBeFilled = false;
                }

                savedMovingGlasses.Clear();
                for (int j = 0; j < movingGlasses.Count; j++)
                {
                    savedMovingGlasses.Add(movingGlasses[j]);
                    Debug.LogError($"Move: Saved MovingGlass {movingGlasses[j].currentID}");
                }

                movingGlasses.Clear();
                Debug.LogError("Move: Cleared current movingGlasses list.");

                StartIteration();
                Debug.LogError("Move: Called StartIteration from Move.");
            }

            if (glass.time > 1.1f)
            {
                Debug.LogError($"Move: Glass {i} reached final stage of movement (time > 1.1). Finalizing.");

                for (int j = 0; j < savedMovingGlasses.Count; j++)
                {
                    savedMovingGlasses[j].time = 0;
                    savedMovingGlasses[j].moving = false;
                    savedMovingGlasses[j].previousPosition = savedMovingGlasses[j].currentPosition;
                    Debug.LogError($"Move: Finalized Glass {savedMovingGlasses[j].currentID}, now at {savedMovingGlasses[j].currentPosition}");
                }

                for (int j = 0; j < currentGlassNoDifficulty; j++)
                {
                    savedPlaces[j].filled = true;
                    savedPlaces[j].currentGlass = savedMovingGlasses[j];
                    savedPlaces[j].comingGlass = null;
                    Debug.LogError($"Move: Updated Place {savedPlaces[j].ID} with new glass.");
                }

                hasStartedNextIteration = false;
                Debug.LogError("Move: Next iteration flag reset.");
            }
        }
        else if (!glass.moving)
        {
            if (glass.hasRunCodeOnce)
                Debug.LogError($"Move: Glass {i} stopped moving. Resetting hasRunCodeOnce.");

            glass.hasRunCodeOnce = false;
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
    public Vector3 previousPosition;
    public Vector3 nextPosition;
    public Vector3 currentPosition;
    public bool moving;
    public bool hasRunCodeOnce;
    public float time;
    public int dirOfRoation;

    public Glass(GameObject glass, int id)
    {
        glassG = glass;
        previousPosition = glassG.transform.localPosition;
        currentPosition = glassG.transform.localPosition;
        previousID = id;
        currentID = id;
        dirOfRoation = (0 == UnityEngine.Random.Range(0, 2) ? -1 : 1);
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

}
