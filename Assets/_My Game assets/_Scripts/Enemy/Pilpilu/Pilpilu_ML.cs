using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;

public class Pilpilu_ML : Agent
{
    [Header("References")]
    Rigidbody rb;
    public ParticleSystem HitEffect;
    List<ParticleSystem> DeleteAfterPlay = new();
    public List<Transform> playerPos = new();
    public Transform posContainer;
    public NavMeshAgent navMeshAgent;

    [Header("Abilites")]
    public float walkingForce;
    public float maxSpeed;
    public float maxJumpHeight;
    public float yRotationSpeed = 360f;
    public float shootRange = 100f;
    public float Gravity;

    [Header("Actions")]
    public float YRotation;
    public float moveForward;
    public float jumpSignal;
    public int action1;
    public bool fire;
    public bool findPlayer;

    [Header("Obsrevations")]
    public List<Vector3> guidingPositions = new();

    [Header("Var for Actions")]
    public bool isGrounded;
    public bool fired;
    public Vector3 toGuide;
    public List<Vector3> allGuidingPositions = new();
    [Tooltip("this is the index from where all guiding pos give positions to guiding positions")] public int showingIndex = 0;


    [Header("Properties")]
    [Tooltip("this is the distance at which agent reaches from the guiding pos after which guiding pos gives new positions")]public int goalRadius = 3;
    // small cooldown after jump //
    public float jumpTime = 0;
    public float jumpTimer = 0.05f;
    // small cooldown after jump //
    public float fireTime = 0;
    public float fireTimer = 3f;
    // cooldown for giving player position//
    public float playerPosTime = 0;
    public float playerPosTimer = 10f;




    // small cooldown after jump (Testing)//
    public float playerTime = 0;
    public float playerTimer = 15f;





    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        if (posContainer == null) return;
        for (int i = 0; i < posContainer.childCount; i++)
        {
            playerPos.Add(posContainer.GetChild(i));
        }
        for (int i = 0; i < 3; i++)
        {
            guidingPositions.Add(Vector3.zero);
        }
    }
    private void Update()
    {
        TimeInc();
        if (!GameManager.Instance.serverStarted)
            return;
        
        if (!rb.useGravity)
        {
            rb.useGravity = true;
        }

        ReadAction0();
        
        FireManager();

        GivePlayerPosManager();
        CheckIfPlayerReachedGuidingPos();
        DeleteParticleSystem();
        GiveRewards();

        //(Testing)
        //RandomizePlayers();
    }


    private void FixedUpdate()
    {
        Movement(); 
        if (!GameManager.Instance.serverStarted)
        {
            return;
        }
        rb.AddForce(Vector3.down * Gravity);
    }


    private void ReadAction0()
    {
        switch (action1)
        {
            case 0: break;
            case 1: //run
                    break;
            case 2: //Disguize
                    break;
            case 3: //fire = true;
                    break;
            case 4: findPlayer = true;
                    break;
            case 5: //find hiding Places
                    break;
        }
    }
    





    private void TimeInc()
    {
        jumpTime += Time.deltaTime;
        fireTime += Time.deltaTime;
        playerTime += Time.deltaTime;
        playerPosTime += Time.deltaTime;

    }
    private void FireManager()
    {
        if (fire)
        {
            fire = false;
            if (fireTime > fireTimer)
                fired = false;
            if (!fired)
            {
                fireTime = 0;
                fired = true;
                Fire();
            }
            else
            {
                AddReward(-0.01f);
            }
        }
    }
    private void GivePlayerPosManager()
    {
        if (playerPosTime > playerPosTimer && findPlayer)
        {
            playerPosTime = 0;
            findPlayer = false;
            showingIndex = 0;
            Vector3 toGuide_ = Vector3.zero;
            int count = GameManager.Instance.connectedClientsData.Count;
            if (GameManager.Instance.serverStarted)
            {
                toGuide_ = GameManager.Instance.connectedClientsData.ElementAtOrDefault(Random.Range(0, count)).playerGameobject.transform.position;
            }
            GivePosIn_ToGuidePos(toGuide_);
        }
    }
    private void CheckIfPlayerReachedGuidingPos()
    {
        for (int i = 0; i < guidingPositions.Count; i++)
        {
            if ((transform.position - guidingPositions[i]).magnitude < goalRadius)
            {
                AddReward(50);
                showingIndex++;
                ChangeGuidingPos();
            }
        }

        void ChangeGuidingPos()
        {
            for (int j = 0; j < guidingPositions.Count; j++)
            {
                int targetIndex = showingIndex + j;

                if (targetIndex < allGuidingPositions.Count)
                    guidingPositions[j] = allGuidingPositions[targetIndex];
                else
                    guidingPositions[j] = Vector3.zero;
            }
        }

    }
    private void DeleteParticleSystem()
    {
        if (DeleteAfterPlay.Count > 0)
        {
            for (int i = 0; i < DeleteAfterPlay.Count; i++)
            {
                if (DeleteAfterPlay[i] != null && DeleteAfterPlay[i].isStopped)
                {
                    Destroy(DeleteAfterPlay[i].gameObject);
                }
                if (DeleteAfterPlay[i] == null)
                {
                    DeleteAfterPlay.RemoveAt(i);
                }
            }
        }

    }
    private void GivePosIn_ToGuidePos(Vector3 pos)
    {
        toGuide = pos;
        Guide();
    }
    private void Guide()
    {
        if (toGuide == Vector3.zero)
        {
            int c = Mathf.Min(guidingPositions.Count, allGuidingPositions.Count);
            for (int i = 0; i < c; i++)
            {
                guidingPositions[i] = Vector3.zero;
            }
            return;
        }
            

        navMeshAgent.SetDestination(toGuide);
        allGuidingPositions = navMeshAgent.path.corners.ToList();
        navMeshAgent.isStopped = true;
        int count = Mathf.Min(guidingPositions.Count, allGuidingPositions.Count);
        for (int i = 0; i < count; i++)
        {
            guidingPositions[i] = allGuidingPositions[i];
        }
    }
    private void RandomizePlayers()
    {
        if (posContainer == null) return;
        if (playerTime > playerTimer)
        {
            playerTime = 0;
            if (GameManager.Instance.serverStarted)
            {
                float count = GameManager.Instance.connectedClientsData.Count;
                for (int i = 0; i < count; i++)
                {
                    GameManager.Instance.connectedClientsData.ElementAtOrDefault(i).playerGameobject.gameObject.transform.position = playerPos[Random.Range(0, playerPos.Count)].position;
                }
            }
        }
    }








    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(rb.linearVelocity.magnitude);
        sensor.AddObservation(guidingPositions[0]);
        sensor.AddObservation(guidingPositions[1]);
        sensor.AddObservation(guidingPositions[2]);
        for (int i = 0; i < 184; i++)
        {
            sensor.AddObservation(0f);
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;
        continuousActions[0] = Input.GetAxis("Mouse X");
        continuousActions[1] = Input.GetAxis("Vertical");
        //continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : -1f;
        //discreteActions[0] = Input.GetKey(KeyCode.F) ? 1 : 0;
        discreteActions[1] = Input.GetKey(KeyCode.Alpha4) ? 4 : 0;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        YRotation = actions.ContinuousActions[0];
        moveForward = actions.ContinuousActions[1];
        //jumpSignal = (actions.ContinuousActions[2] + 1) / 2 * maxJumpHeight;
        action1 = actions.DiscreteActions[1];
    }





    private void Movement()
    {
        transform.Rotate(0, YRotation * Time.fixedDeltaTime * yRotationSpeed, 0);
        Vector3 moveDirection = transform.forward * moveForward * walkingForce;
        rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z));
        Vector2 velocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.y);
        }


        if (isGrounded && jumpSignal > 0.1f && jumpTime > jumpTimer)
        {
            jumpTime = 0;
            isGrounded = false;
            rb.AddForce(Vector3.up * jumpSignal, ForceMode.Impulse);
        }
    }
    private void Fire()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            ParticleSystem particle = Instantiate(HitEffect, hit.point, Quaternion.identity);
            particle.Play();
            DeleteAfterPlay.Add(particle);
            if (hit.collider.CompareTag("Player"))
            {
                AddReward(10);
                //DamagePLayer TODO
                EndEpisode();
            }
            else
            {
                AddReward(-0.5f);
            }
        }
    }
    private void GiveRewards()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                AddReward(5f * Time.deltaTime);
                EndEpisode();
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * shootRange, Color.red);
            // Optional penalty
            // AddReward(-0.5f * Time.deltaTime);
        }

        // --------- Reward for being near the target position ---------
        float distance = Vector3.Distance(transform.position, guidingPositions[0]);

        float maxReward = 1f;
        float maxEffectiveDistance = 50f;
        float clampedDistance = Mathf.Clamp(distance, 0f, maxEffectiveDistance);
        float distanceReward = maxReward * (1f - (clampedDistance / maxEffectiveDistance));

        AddReward(distanceReward * Time.deltaTime);

        // --------- Reward for looking toward the target ---------
        Vector3 directionToTarget = (guidingPositions[0] - transform.position).normalized;
        float alignment = Vector3.Dot(transform.forward.normalized, directionToTarget);

        // Dot is 1 if perfectly aligned, -1 if opposite, 0 if perpendicular
        float lookReward = Mathf.Clamp01(alignment); // 0 to 1

        AddReward(lookReward * 0.5f * Time.deltaTime); // Scale as needed
    }






    void OnDrawGizmosSelected()
    {
        if (navMeshAgent == null || navMeshAgent.path == null)
            return;

        Vector3[] corners = navMeshAgent.path.corners;
        Gizmos.color = Color.cyan;

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.1f);
        }

        if (corners.Length > 0)
        {
            Gizmos.DrawSphere(corners[corners.Length - 1], 0.1f);
        }
    }
}
