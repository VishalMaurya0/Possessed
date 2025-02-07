using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    public Vector3 eyePosition = new(0f, 0.5f, 0f);

    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 6.0f;
    public float crouchSpeed = 1.5f;
    public float rotationSpeed = 5.0f;

    [Header("Mouse Settings")]
    public float lookSensitivity = 2.0f;
    public float maxLookAngle = 80f;

    [Header("Stamina Settings")]
    public float maxStamina = 10.0f;
    public float staminaRecoveryRate = 2.0f;
    public float XfasterStaminaRecoveryRate = 1.3f;
    public float staminaDepletionRate = 2.0f;

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
}
