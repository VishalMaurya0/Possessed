using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClueGathering : MonoBehaviour
{
    [Header("References")]
    public RectTransform elementParent;
    public GameObject buttonElementPrefab;
    public List<Color> colors = new();
    public RectTransform BigCurser;
    public RectTransform SmallCurser;
    public GameObject procedureImageParent;
    List<RectTransform> procedureImages = new();

    [Header("Properties")]
    public List<GameObject> allButtonElements = new();
    public List<Image> allButtonElementsChildImages = new();
    public List<int> buttonValues = new();
    public List<int> addedButtonValues = new(8);
    public int maxIndex;
    public int secondMaxIndex;

    [Header("For Animation")]
    private float moveDuration = 0.5f;

    private bool isMoving = false;
    private float moveElapsed = 0f;
    private Vector3 startPos;
    private Vector3 targetPos; 
    
    private bool isSmallMoving = false;
    private float smallMoveElapsed = 0f;
    private Vector3 smallStartPos;
    private Vector3 smallTargetPos;



    private void Start()
    {
        for (int i = 0; i < 32; i++)
        {
            allButtonElements.Add(Instantiate(buttonElementPrefab, elementParent));
            allButtonElements[i].SetActive(true);
            int a = i;
            allButtonElements[i].GetComponentInChildren<Button>().onClick.AddListener(() => { ChangeButtonValue(a); });
            allButtonElementsChildImages.Add(allButtonElements[i].GetComponentInChildren<Button>().gameObject.GetComponent<Image>());
            buttonValues.Add(0);
        }

        for (int i = 0; i < procedureImageParent.transform.childCount; i++)
        {
            procedureImages.Add(procedureImageParent.transform.GetChild(i).GetComponent<RectTransform>());
        }
    }

    private void Update()
    {
        for (int i = 0; i < 8; i++)
        {
            addedButtonValues[i] = buttonValues[i];
            addedButtonValues[i] += buttonValues[i + 8];
            addedButtonValues[i] += buttonValues[i + 8 + 8];
            addedButtonValues[i] += buttonValues[i + 8 + 8 + 8];
        }


        maxIndex = FindMaxIndexIfAvailable();
        if (maxIndex != -1)
        {
            BigCurser.gameObject.SetActive(true);
            Vector3 desiredPos = new(procedureImages[maxIndex].position.x, BigCurser.position.y, BigCurser.position.z);
            if (!isMoving || targetPos != desiredPos)
            {
                StartCursorMove(desiredPos);
            }
        }
        else { BigCurser.gameObject.SetActive (false); }


        secondMaxIndex = FindSecondMaxIndexIfAvailable();
        if (secondMaxIndex != -1)
        {
            SmallCurser.gameObject.SetActive(true);
            Vector3 desiredPos = new(procedureImages[secondMaxIndex].position.x, SmallCurser.position.y, SmallCurser.position.z);
            if (!isSmallMoving || smallTargetPos != desiredPos)
            {
                StartSmallCursorMove(desiredPos);
            }
        }
        else { SmallCurser.gameObject.SetActive (false); }

        // Animate big cursor
        if (isMoving)
        {
            moveElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(moveElapsed / moveDuration);
            float easedT = EaseOutCubic(t);
            BigCurser.position = Vector3.Lerp(startPos, targetPos, easedT);

            if (t >= 1f)
                isMoving = false;
        }

        // Animate small cursor
        if (isSmallMoving)
        {
            smallMoveElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(smallMoveElapsed / moveDuration);
            float easedT = EaseOutCubic(t);
            SmallCurser.position = Vector3.Lerp(smallStartPos, smallTargetPos, easedT);

            if (t >= 1f)
                isSmallMoving = false;
        }
    }



    private void StartCursorMove(Vector3 newTargetPos)
    {
        startPos = BigCurser.position;
        targetPos = newTargetPos;
        moveElapsed = 0f;
        isMoving = true;
    }
    private void StartSmallCursorMove(Vector3 newTargetPos)
    {
        smallStartPos = SmallCurser.position;
        smallTargetPos = newTargetPos;
        smallMoveElapsed = 0f;
        isSmallMoving = true;
    }




    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3);
    }




    private int FindMaxIndexIfAvailable()
    {
        // Finding Max
        int maxVal = int.MinValue;
        int maxIndex = -1;
        int count = 0;

        for (int i = 0; i < addedButtonValues.Count; i++)
        {
            if (addedButtonValues[i] > maxVal)
            {
                maxVal = addedButtonValues[i];
                maxIndex = i;
                count = 1;
            }
            else if (addedButtonValues[i] == maxVal)
            {
                count++;
            }
        }

        // Return the index only if the max value is unique
        return count == 1 ? maxIndex : -1;
    }
    private int FindSecondMaxIndexIfAvailable()
    {
        int first = int.MinValue, second = int.MinValue;
        int firstIndex = -1, secondIndex = -1;

        // Step 1: Find max and second max values
        for (int i = 0; i < addedButtonValues.Count; i++)
        {
            int val = addedButtonValues[i];

            if (val > first)
            {
                second = first;
                secondIndex = firstIndex;
                first = val;
                firstIndex = i;
            }
            else if (val > second && val != first)
            {
                second = val;
                secondIndex = i;
            }
        }

        // Step 2: Check uniqueness of second max
        int count = 0;
        for (int i = 0; i < addedButtonValues.Count; i++)
        {
            if (addedButtonValues[i] == second)
                count++;
        }

        return (secondIndex != -1 && count == 1) ? secondIndex : -1;
    }




    private void ChangeButtonValue(int a)
    {
        buttonValues[a]++;
        if (buttonValues[a] >= 3)
        {
            buttonValues[a] = 0;
        }
        allButtonElementsChildImages[a].color = colors[buttonValues[a]];
    }
}
