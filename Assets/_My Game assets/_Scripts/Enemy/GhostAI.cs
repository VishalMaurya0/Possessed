using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class GhostAI : NetworkBehaviour
{
    public GhostState Currentstate;
    public GhostState RoamingState;
    public GhostState HuntingState;
    public GhostState DyingState;

    public NavMeshAgent navMeshAgent;
    public GhostData ghostData;
    public bool isHunting;
    public bool stopHunt;
    public bool photoClicked;

    public float huntToStartTimer = 0;
    float timeBetweenHuntDuration;

    public TMP_Text text;


    private void Update()
    {
        if (!IsServer) return;
        if (navMeshAgent == null)
        {
            huntToStartTimer = 0;
            timeBetweenHuntDuration = ghostData.timeBetweenHuntDuration + Random.Range(-ghostData.timeBetweenHuntDurationRange, ghostData.timeBetweenHuntDurationRange);
            RoamingState = new GhostRoamingState(this);
            HuntingState = new GhostHuntingState(this);
            DyingState = new GhostDyingState(this);
            navMeshAgent = this.GetComponent<NavMeshAgent>();
            ChangeState(RoamingState);
        }



        Currentstate.UpdateState();


        //----------------------------------------------Hunt Time--------------//
        huntToStartTimer += Time.deltaTime;
        if (huntToStartTimer > timeBetweenHuntDuration && !isHunting)
        {
            huntToStartTimer = 0;
            ChangeState(HuntingState);
        }
        if (stopHunt)
        {
            ChangeState(RoamingState);
            stopHunt = false;
        }
    }

    public void ChangeState(GhostState newState)
    {
        Currentstate?.ExitState();
        Currentstate = newState;
        Currentstate?.EnterState();
        text.text = newState.ToString();
    }

    public Vector3[] FindPlayersPosition()
    {
        Vector3[] pos = new Vector3[GameManager.Instance.connectedClients.Count];
        int index = 0;
        foreach (var player in GameManager.Instance.connectedClients)
        {
            if (player.Value == null)
            {
                return default;
            }
            pos[index] = player.Value.transform.position;
            index++;
        }
        return pos;
    }

    public bool CheckPlayerVisibility(out KeyValuePair<ulong, GameObject> player)
    {
        player = default;

        if (GameManager.Instance == null || GameManager.Instance.gameEnd || ghostData == null)
            return false;

        Vector3[] allPlayersPositions = FindPlayersPosition();
        if (allPlayersPositions == null || allPlayersPositions.Length == 0)
            return false;

        Dictionary<ulong, GameObject> players = new();

        foreach (var pos in allPlayersPositions)
        {
            Vector3 targetDir = pos - transform.position;
            Vector3 targetPos = pos - (transform.position + (ghostData?.eyePosition ?? Vector3.zero)) + Vector3.down * 0.25f;
            Vector3 lookDir = transform.forward;

            targetDir.Normalize();
            lookDir.Normalize();

            float angle = Vector3.Angle(lookDir, targetDir);
            if (angle < 60)
            {
                if (RaycastCheckIfPlayerIsVisible(targetDir, targetPos, out player) && player.Value != null && !players.ContainsKey(player.Key))
                {
                    players.Add(player.Key, player.Value);
                }
            }
        }

        if (players.Count > 0)
        {
            KeyValuePair<ulong, GameObject> finalPlayer = default;
            float minDis = float.MaxValue;

            foreach (var playerr in players)
            {
                if (playerr.Value != null)
                {
                    float distance = (playerr.Value.transform.position - transform.position).sqrMagnitude;
                    if (distance < minDis)
                    {
                        minDis = distance;
                        finalPlayer = playerr;
                    }
                }
            }

            if (finalPlayer.Value != null)
            {
                player = finalPlayer;
                return true;
            }
        }

        return false;
    }


    public bool RaycastCheckIfPlayerIsVisible(Vector3 targetDir, Vector3 targetPos, out KeyValuePair<ulong, GameObject> player)
    {
        Vector3 rayOrigin = transform.position + ghostData.eyePosition;
        Debug.DrawRay(rayOrigin, targetDir * ghostData.ghostLookDistance, Color.red, 1f);

        if (Physics.Raycast(rayOrigin, targetDir, out RaycastHit hit, ghostData.ghostLookDistance))
        {
            Debug.DrawRay(rayOrigin, targetDir * hit.distance, Color.green, 1f);  // Green ray to hit point
            Debug.DrawRay(hit.point, Vector3.up * 1f, Color.blue, 1f);
            foreach (var playerr in GameManager.Instance.connectedClients)
            {
                if (hit.collider.gameObject == playerr.Value)
                {
                    player = playerr;
                    return true;
                }
            }
        }
        
        if (Physics.Raycast(rayOrigin, targetPos, out RaycastHit hit2, ghostData.ghostLookDistance))
        {
            foreach (var playerr in GameManager.Instance.connectedClients)
            {
                if (hit2.collider.gameObject == playerr.Value)
                {
                    Debug.Log("player found");
                    player = playerr;
                    return true;
                }
            }
        }
        
        player = default;
        return false;
    }
}
