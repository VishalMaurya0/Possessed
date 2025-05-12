using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TaskForCoins : MonoBehaviour
{
    [Header("Difficulty Increment Settings")]
    [SerializeField] public float IterationDiffIncrementRate = 1/2;
    [SerializeField] public float GlassNoDiffiIncrementRate = 1/5;
    [SerializeField] public float TimeDiffiIncrementRate = 1/30;
    [SerializeField] public float MinTimeDiffiItCanHandle = 0.3f;
    [SerializeField] public float MaxGlassNoItCanHandle = 4;

    [Header("Restart Time Settings")]
    public bool restart = false;
    public bool positionReset = false;


    [Header("Task Settings")]
    [SerializeField] float startingIterationDifficulty = 6;
    [SerializeField] float startingGlassNoDifficulty = 1;
    [SerializeField] float startingTimeDifficulty = 1;
    [SerializeField] bool gameStarted = false;
    [SerializeField] bool endTheGame = false;
    [SerializeField] public int gameEndsAfetrLastIter = 1;


    [Header("Each Iteration Settings")]
    [SerializeField] float iterationLeft;
    [SerializeField] float currentIterationDifficulty = 6;
    [SerializeField] float currentGlassNoDifficulty = 1;
    [SerializeField] float currentTimeDifficulty = 1;
    bool hasStartedNextIteration = false;          //------To run Once


    [Header("Debug")]
    [SerializeField] Material FilledTOBeFilled;
    [SerializeField] Material FilledNOTTOBeFilled;
    [SerializeField] Material NOTFilledNOTTOBeFilled;
    [SerializeField] Material NOTFilledTOBeFilled;
    public GameObject placesContainer;


    [Header("Instantiated Data")]
    [SerializeField] List<Vector3> InitialPos = new();
    [SerializeField] List<Glass> movingGlasses = new();
    [SerializeField] List<Glass> savedMovingGlasses = new();
    [SerializeField] List<Place> nextPlaces = new();
    [SerializeField] List<Place> savedPlaces = new();
    [SerializeField] GameObject glassContainer;
    [SerializeField] List<Glass> glasses = new();
    [SerializeField] List<Place> places = new();

    [Header("References")]
    [SerializeField] public List<CupForCoinTask> cupForCoinTasks = new();

    private void Start()
    {
        currentIterationDifficulty = startingIterationDifficulty;
        currentGlassNoDifficulty = startingGlassNoDifficulty;
        currentTimeDifficulty = startingTimeDifficulty;
        for (int i = 0; i < glassContainer.transform.childCount; i++)
        {
            InitialPos.Add(glassContainer.transform.GetChild(i).localPosition);
            cupForCoinTasks.Add(glassContainer.transform.GetChild(i).GetComponent<CupForCoinTask>());
        }

    }

    private void Update()
    {
        for (int i = 0; i < glasses.Count; i++)
        {
            Move(i);
        }

        if (restart)
        {
            restart = false;
            ResetGameAndRestart();
        }

        if (positionReset)
        {
            ResetPos();
        }
        DebugPlaces();
    }

    private void DebugPlaces()
    {
        foreach (Place place in places)
        {
            Material mat = null;

            if (place.filled && place.toBeFilled)
                mat = FilledTOBeFilled;
            if (!place.filled && place.toBeFilled)
                mat = NOTFilledTOBeFilled;
            if (place.filled && !place.toBeFilled)
                mat = FilledNOTTOBeFilled;
            if (!place.filled && !place.toBeFilled)
                mat = NOTFilledNOTTOBeFilled;


            place.GameObject.material = mat;
        }
    }

    public void IncreaseDifficulty()
    {
        startingIterationDifficulty += IterationDiffIncrementRate;
        startingGlassNoDifficulty += GlassNoDiffiIncrementRate;

        if (startingGlassNoDifficulty > MaxGlassNoItCanHandle)
        {
            startingGlassNoDifficulty = MaxGlassNoItCanHandle;
        }

        startingTimeDifficulty -= TimeDiffiIncrementRate;
        if (startingTimeDifficulty < MinTimeDiffiItCanHandle)
        {
            startingTimeDifficulty = MinTimeDiffiItCanHandle;
        }
    }

    private void ResetPos()
    {
        gameStarted = false;
        endTheGame = false;
        gameEndsAfetrLastIter = 1;
        
        currentIterationDifficulty = startingIterationDifficulty;
        currentGlassNoDifficulty = startingGlassNoDifficulty;
        currentTimeDifficulty = startingTimeDifficulty;
        movingGlasses.Clear();
        savedMovingGlasses.Clear();
        nextPlaces.Clear();
        savedPlaces.Clear();
        glasses.Clear();
        places.Clear();
        hasStartedNextIteration = false;

        //===== New Glasses Instances =====//
        glasses.Clear();
        for (int i = 0; i < glassContainer.transform.childCount; i++)
        {
            Glass glass = new Glass(glassContainer.transform.GetChild(i).gameObject, i, InitialPos[i], positionReset);
            glasses.Add(glass);
            places.Add(new Place(i, glasses[i]));
        }
        positionReset = false;
    }

    private void ResetGameAndRestart()
    {
        gameStarted = false;
        endTheGame = false;
        gameEndsAfetrLastIter = 1;

        currentIterationDifficulty = startingIterationDifficulty;
        currentGlassNoDifficulty = startingGlassNoDifficulty;
        currentTimeDifficulty = startingTimeDifficulty;
        movingGlasses.Clear();
        savedMovingGlasses.Clear();
        nextPlaces.Clear();
        savedPlaces.Clear();
        glasses.Clear();
        places.Clear();
        hasStartedNextIteration = false;

        //===== New Glasses Instances =====//
        glasses.Clear();
        for (int i = 0; i < glassContainer.transform.childCount; i++)
        {
            Glass glass = new Glass(glassContainer.transform.GetChild(i).gameObject, i, InitialPos[i], positionReset);
            glasses.Add(glass);
            places.Add(new Place(i, glasses[i]));
            places[i].GameObject = placesContainer.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>();
        }
        positionReset = false;

        // ============== Start =============//
        if (!gameStarted)
        {
            gameStarted = true;
            iterationLeft = currentIterationDifficulty;
            StartIteration();
        }
    }


    private void StartIteration()
    {
        if (!gameStarted)
        {
            return;
        }

        if ((int)iterationLeft == 0)
        {
            iterationLeft--;
            LastIteration();
            return;
        }
        else if ((int)iterationLeft <= 0)
        {
            return;
        }
        
        iterationLeft--;
        
        Debug.Log($"StartIteration: Iteration {startingIterationDifficulty - iterationLeft}/{startingIterationDifficulty}");

        for (int i = 0; i < currentGlassNoDifficulty; i++)
        {
            FillMovingGlassesAtStart(0);
        }

        for (int i = 0; i < movingGlasses.Count; i++)
        {
            SelectPlaceToGoAtStart(0, i);
            movingGlasses[i].currentID = nextPlaces[i].ID;
            nextPlaces[i].comingGlass = movingGlasses[i];
            if (savedMovingGlasses.Count > 0)
            {
                movingGlasses[i].dirOfRoation = (-1) * savedMovingGlasses[i].dirOfRoation;
            }
        }

        for (int i = 0; i < movingGlasses.Count; i++)
        {
            movingGlasses[i].moving = true;
        }
    }

    private void LastIteration()
    {
        if (!gameStarted)
        {
            return;
        }

        for (int i = 0; i < nextPlaces.Count; i++)
        {
            nextPlaces[i].comingGlass = savedMovingGlasses[i];
        }

        endTheGame = true;
        movingGlasses.Clear(); //  Prevent accumulation

        while (nextPlaces.Count > 0)
        {
            int ID = nextPlaces[0].ID;

            if (ID < 0 || ID >= places.Count)
            {
                Debug.LogError($"[LastIteration] ERROR: Invalid ID {ID} - out of bounds of places list.");
                return;
            }

            if (places[ID].filled && places[ID].currentGlass != null && places[ID].currentGlass.glassG != null)
            {
                movingGlasses.Add(places[ID].currentGlass);
            }
            nextPlaces.RemoveAt(0);
        }

        //  Improved null check
        for (int i = 0; i < places.Count; i++)
        {
            if ((places[i].currentGlass == null || places[i].currentGlass.glassG == null) &&
                (places[i].comingGlass == null || places[i].comingGlass.glassG == null))
            {
                nextPlaces.Add(places[i]);
            }

        }

        if (movingGlasses.Count > nextPlaces.Count)
        {
            positionReset = true;
            Debug.LogError($"[LastIteration] ERROR: Not enough empty places to move all glasses. movingGlasses: {movingGlasses.Count}, nextPlaces: {nextPlaces.Count}");
            return;
        }

        for (int i = 0; i < movingGlasses.Count; i++)
        {
            Debug.Log($"[LastIteration] Moving glass {i} to place ID {nextPlaces[i].ID}");
            movingGlasses[i].currentID = nextPlaces[i].ID;
            nextPlaces[i].comingGlass = movingGlasses[i];
            movingGlasses[i].nextPosition = nextPlaces[i].myPosition;
        }

        for (int i = 0; i < movingGlasses.Count; i++)
        {
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
                places[ID].currentGlass = null;
                nextPlaces.RemoveAt(0);
                return;
            }

            nextPlaces.RemoveAt(0);
        }

        int randomID = UnityEngine.Random.Range(0, places.Count);

        if (places[randomID].filled && !places[randomID].currentGlass.moving && !movingGlasses.Contains(places[randomID].currentGlass))
        {
            movingGlasses.Add(places[randomID].currentGlass);
            places[randomID].filled = false;     
            places[randomID].currentGlass = null;

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
        if (!places[randomID].toBeFilled && (movingGlasses[i].currentID != randomID))
        {
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
        if (!gameStarted)
        {
            return;
        }

        Glass glass = glasses[i];

        if (glass.moving && glass.time < currentTimeDifficulty*1.8f)
        {
            float t = glass.time > currentTimeDifficulty ? currentTimeDifficulty : glass.time;
            glass.time += Time.deltaTime;

            Vector3 change = glass.nextPosition - glass.previousPosition;

            if (change.magnitude == 0)
            {
                restart = true;
                positionReset = true;
                Debug.LogError($"[LastIteration] ERROR: Not enough empty places to move all glasses. movingGlasses: {movingGlasses.Count}, nextPlaces: {nextPlaces.Count}");
                return;
            }

            float sign = change.x / change.magnitude * (-1);

            float fractionChangeInT = t / currentTimeDifficulty;

            glass.currentPosition = (glass.previousPosition + glass.nextPosition) / 2 +
                new Vector3(
                change.magnitude / 2 * sign * MathF.Cos(fractionChangeInT * MathF.PI), 
                0, 
                change.magnitude / 2 * glass.dirOfRoation * MathF.Sin(fractionChangeInT * MathF.PI)
                );

            if (glass.time > currentTimeDifficulty*0.7f && !hasStartedNextIteration && !glass.hasRunCodeOnce)
            {
                for (int j = 0; j < movingGlasses.Count; j++)
                {
                    movingGlasses[j].hasRunCodeOnce = true;
                }

                savedPlaces.Clear();
                hasStartedNextIteration = true;

                for (int j = 0; j < nextPlaces.Count; j++)
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

            if (glass.time > currentTimeDifficulty*1.1f && gameEndsAfetrLastIter > 0)
            {

                for (int j = 0; j < savedMovingGlasses.Count; j++)
                {
                    savedMovingGlasses[j].time = 0;
                    savedMovingGlasses[j].moving = false;
                    savedMovingGlasses[j].previousPosition = savedMovingGlasses[j].currentPosition;
                }

                for (int j = 0; j < savedPlaces.Count; j++)
                {
                    savedPlaces[j].filled = true;
                    savedPlaces[j].currentGlass = savedMovingGlasses[j];
                    if (!savedPlaces[j].toBeFilled)
                    {
                        savedPlaces[j].comingGlass = null;
                    }
                }

                if (endTheGame)
                {
                    gameEndsAfetrLastIter--;
                    cupForCoinTasks.ForEach(task => { task.clickable = true; });
                    cupForCoinTasks.ForEach(task => { task.getCoin = true; });
                }

                hasStartedNextIteration = false;
            }
            glasses[i].glassG.transform.localPosition = glasses[i].currentPosition;

        }
        else if (!glass.moving)
        {
            glass.hasRunCodeOnce = false;
        }

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

    public Glass(GameObject glass, int id, Vector3 initialPos, bool wasLastIterWrong)
    {
        glassG = glass;
        if (wasLastIterWrong)
        {
            glass.transform.localPosition = initialPos;
        }
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
    public MeshRenderer GameObject;
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
