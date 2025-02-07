using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectingThreeProcedures : NetworkBehaviour
{
    public int[] barometerReadings = new int[8], giegerCounterReadings = new int[8], EMFReadings = new int[8], FSCReadings = new int[8], totalReadings = new int[8];
    public int[] threeProcedureIndex;

    bool notifyClients;


    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        threeProcedureIndex = CheckReadings(totalReadings);
        threeProcedureIndex[2] = SelectThirdProcedure(threeProcedureIndex[0], threeProcedureIndex[1]);
        if (IsServer)
            GameManager.Instance.selectedProceduresIndex = threeProcedureIndex;

        notifyClients = true;
    }

    private void Singleton_OnServerStarted()
    {
        if (notifyClients)
        {
            NotifyClientsAboutSelectedProceduresClientRpc(threeProcedureIndex);
            if (IsServer)
                GameManager.Instance.selectedProceduresIndex = threeProcedureIndex;
            notifyClients = false;
        }
    }


    [ClientRpc]
    private void NotifyClientsAboutSelectedProceduresClientRpc(int[] ints)
    {
        GameManager.Instance.selectedProceduresIndex = ints;
    }

    private void SetReadings(int[] readings)
    {
        int x;
        List<int> list = new();
        for (int i = 0; i < 8; i++)
        {
            list.Add(i);
        }



        x = Random.Range(0, list.Count);
        int a = list[x];
        list.RemoveAt(x);

        x = Random.Range(0, list.Count);
        int b = list[x];
        list.RemoveAt(x);

        readings[a] = 0;
        readings[b] = 0;



        int[] C = new int[6];
        for (int i = 0; i < 4; i++)
        {
            x = Random.Range(0, list.Count);
            C[i] = list[x];
            list.RemoveAt(x);
            readings[C[i]] = 1;
        }
        for (int i = 4; i < 6; i++)
        {
            x = Random.Range(0, list.Count);
            C[i] = list[x];
            list.RemoveAt(x);
            readings[C[i]] = 2;
        }
    } 


    private int[] CheckReadings(int[] totalReadings)
    {
        int[] a;
        int max = totalReadings[0], secondMax = 0;
        int maxIndex = 0, secMaxIndex = 0;

        for (int i = 1; i < totalReadings.Length; i++)
        {
            if (totalReadings[i] > max)
            {
                secondMax = max;
                secMaxIndex = maxIndex;
                max = totalReadings[i];
                maxIndex = i;
            }else if(totalReadings[i] > secondMax)
            {
                secondMax = totalReadings[i];
                secMaxIndex = i;
            }
        }

        //if max is triple
        int maxCount = 0, secMaxCount = 0;


        for (int i = 0; i < totalReadings.Length; i++)
        {
            if (totalReadings[i] == max)
                maxCount++;
        }
        for (int i = 0; i < totalReadings.Length; i++)
        {
            if (totalReadings[i] == secondMax)
                secMaxCount++;
        }
        a = new int[] { maxIndex, secMaxIndex, 0 };


        if (maxCount > 2)
        {
            a = RedoReadings();
        }

        // if max is 1 and second max is 2
        if (maxCount == 1)
        {
            if (secMaxCount > 1)
            {
                a = RedoReadings();
            }
        }
        return a;
    }

    private int SelectThirdProcedure(int index1, int index2)
    {
        int index3 = Random.Range(0, 8);
        if (index1 == index3 || index2 == index3)
        {
            Debug.Log($"index3 {index3} is invalid, retrying...");
            return SelectThirdProcedure(index1, index2);
        }
        else if (index1 != index3 && index2 != index3)
        {
            return index3;
        }
        return SelectThirdProcedure(index1, index2);
    }

    private int[] RedoReadings()
    {
        for (int i = 0; i < 8; i++)
        {
            Debug.Log( totalReadings[i] );
        }

        Debug.Log("---------------------------------------------------------------------");

        SetReadings(barometerReadings);
        SetReadings(giegerCounterReadings);
        SetReadings(EMFReadings);
        SetReadings(FSCReadings);

        for (int i = 0; i < 8; i++)
        {
            totalReadings[i] = barometerReadings[i] + giegerCounterReadings[i] + EMFReadings[i] + FSCReadings[i];
        }
        return CheckReadings(totalReadings);

    }
}