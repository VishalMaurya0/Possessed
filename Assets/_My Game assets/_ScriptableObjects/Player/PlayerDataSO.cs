using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    public Vector3 eyePosition = new(0f, 0.5f, 0f);

    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 6.0f;
    public float crouchSpeed = 1f;
    public float rotationSpeed = 5.0f;

    [Header("Mouse Settings")]
    public float lookSensitivity = 2.0f;
    public float maxLookAngle = 80f;

    [Header("Stamina Settings")]
    public float maxStamina = 10.0f;
    public float staminaRecoveryRate = 2.0f;
    public float XfasterStaminaRecoveryRate = 1.3f;
    public float staminaDepletionRate = 2.0f;


    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;
    public float standingHeight = 2.0f;
    public float crouchTransitionSpeed = 10f; // How fast we go up/down
    public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    public Vector3 standingCenter = new Vector3(0, 1f, 0);
    public float camCrouchHeight = 0;
    public float canNormalHeight = 1f;

    [Header("Torch Settings")]
    public Light torchLight;

    [Header("Input Settings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode torchToggleKey = KeyCode.F;

    [Header("Fear Meter")]
    public float normalFearRate = .02f;
    public float watchingGhostFearRate = 1f;
    public float watchingDollFearRate = 0.5f;
    public float ghostWatchingFearRate = 2.5f;
    public float regenFearRate = 1f;
    public float maxFearDistance = 5f;
    public float revivedFear = 80;

    [Header("Noise Settings")]
    public float timeDurationForCalculatingFootNoise = 2f;
    public float walkDist = 5f;
}
