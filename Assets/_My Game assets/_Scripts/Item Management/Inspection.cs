using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;

//using UnityEditor.PackageManager;
using UnityEngine;

public class Inspection : NetworkBehaviour
{
    Camera camera;
    public Transform inspectPoint;
    public float range = 36f;
    private Transform originalParent;
    private Vector3 originalPosition; 
    private Quaternion originalRotation;
    public bool isInspecting = false;

    private readonly float rotationSpeed = 5f;
    private ItemHolding ItemHolding;

    [Header("Glow Settings")]
    [SerializeField] MeshRenderer[] meshRenderers;
    [SerializeField] Material outlineMat;
    float glowScale = 1.15f;

    private Rigidbody rb;
    private NetworkRigidbody netRb;
    // public Collider collider;

    // Store enabled colliders for restoration
    private List<Collider> previouslyEnabledColliders = new List<Collider>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        netRb = GetComponent<NetworkRigidbody>();
        //collider = GetComponent<Collider>();
    }

    private void Start()
    {
        camera = Camera.main;
        meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
    }

    void Update()
    {
        if (isInspecting)
        {
            RotateObject(GameManager.Instance.ownerPlayer);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndInspection();
            }
            if (Input.GetMouseButtonDown(1))
            {
                EndInspection();
            }
        }

        if (ItemHolding == null && GameManager.Instance.ownerPlayer != null)
        {
            ItemHolding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
        }

        if (inspectPoint == null)
        {
            inspectPoint = camera.transform.GetChild(0).transform;
        }
    }

    void OnMouseDown()
    {
        if (!isInspecting && (transform.position - GameManager.Instance.ownerPlayer.transform.position).sqrMagnitude < range)
        {
            AudioManager.PlaySound(AudioType.ItemInspect);
            // ============CASE 1: Client is NOT owner (Standard Client Logic)
            if (GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId && GetComponent<NetworkObject>().OwnerClientId == NetworkManager.ServerClientId)
            {
                GrantPermissionToStartInspectionServerRpc(NetworkManager.Singleton.LocalClientId);
            }
            // ============CASE 2: Host/Local Player IS Owner
            else if (GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                DisableObjectForClientServerRPC();
                StartInspection();
            }
        }
    }

    [ClientRpc]
    void PermissionGrantedClientRpc(ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId)
            return;

        StartInspection();
    }

    #region Inspection Logic
    private void StartInspection()
    {
        isInspecting = true;

        if (ItemHolding == null && GameManager.Instance.ownerPlayer != null)
        {
            ItemHolding = GameManager.Instance.ownerPlayer.GetComponent<ItemHolding>();
        }

        ItemHolding.isInspecting = true;

        this.GetComponent<Rigidbody>().isKinematic = true;
        GameManager.Instance.handlePlayerLookWithMouse = false;
        GameManager.Instance.handleMovement = false;
        GameManager.Instance.itemScrollingLock = true;

        // Store original position and rotation
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;


        // Move object to inspection point
        transform.SetParent(null);
        if (inspectPoint == null)                             //---------------when spawning object with F update does not run so this var is null at that point-------------//
        {
            inspectPoint = Camera.main.transform.GetChild(0).transform;
        }
        transform.position = inspectPoint.position;
        transform.rotation = Quaternion.identity; // Optional: Reset rotation for consistency
    }

    private void RotateObject(GameObject player)
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        if (inspectPoint == null)                             //---------------when spawning object with F update does not run so this var is null at that point-------------//
        {
            inspectPoint = camera.transform.GetChild(0).transform;
        }
        Vector3 dir = new (inspectPoint.transform.position.z - player.transform.position.z, 0f, player.transform.position.x - inspectPoint.transform.position.x);

        // Apply rotation based on mouse movement
        transform.Rotate(Vector3.up, -mouseX, Space.World); // Horizontal rotation
        transform.Rotate(dir, mouseY, Space.World); // Vertical rotation
    }

    public void EndInspection()
    {
        isInspecting = false;

        AudioManager.PlaySound(AudioType.ItemDrop);

        ItemHolding.isInspecting = false;
        Debug.Log("unzooming");

        // Restore original position and rotation
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        if (originalParent?.GetComponent<NetworkObject>() != null)
        {
            transform.transform.SetParent(originalParent);
        }
        else
        {
            Debug.LogWarning("Attempted to parent a NetworkObject under a non-NetworkObject. Parenting aborted.");
        }
        //this.GetComponent<Rigidbody>().isKinematic = false;
        ItemHolding.SetEverythingNormal(false);
        ItemHolding.HandleUnZoom();

        StartCoroutine(RetrievePermission());
    }
    #endregion

    #region RPC Region
    [ServerRpc(RequireOwnership = false)]
    public void GrantPermissionToStartInspectionServerRpc(ulong ClientId)
    {
        if (GetComponent<NetworkObject>().OwnerClientId != ClientId)
        {
            GetComponent<NetworkObject>().ChangeOwnership(ClientId);
        }
        DisableObjectClientRPC();
        PermissionGrantedClientRpc(ClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void DisableObjectForClientServerRPC()
    {
        DisableObjectClientRPC();
    }

    [ClientRpc]
    private void DisableObjectClientRPC()
    {
        //if (GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
            //return;
        netRb.enabled = false;
        //collider.enabled = false;

        // Disable all enabled colliders and store them
        previouslyEnabledColliders.Clear();
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            if (col.enabled)
            {
                previouslyEnabledColliders.Add(col);
                col.enabled = false;
            }
        }

        rb.isKinematic = true;
    }

    [ClientRpc]
    private void EnableObjectClientRPC()
    {
        //if (GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
        //    return;
        netRb.enabled = true;
        rb.isKinematic = false;
        // Re-enable only previously enabled colliders
        foreach (var col in previouslyEnabledColliders)
        {
            if (col != null)
                col.enabled = true;
        }
        previouslyEnabledColliders.Clear();
    }

    [ServerRpc(RequireOwnership = false)]
    void RetrievePermissionServerRpc(ulong ClientId)
    {
        EnableObjectClientRPC();
        GetComponent<NetworkObject>().ChangeOwnership(ClientId);
    }

    IEnumerator RetrievePermission()
    {
        yield return new WaitForSeconds(.5f);  //TODO Retrieve permissin when obhject is back to original position in every client

            RetrievePermissionServerRpc(NetworkManager.ServerClientId);

        yield return null;
    }
    #endregion

    #region Disable Network Rigidbody While Inspecting
    
    #endregion

    #region Glow Effect on Mouse Hover
    void OnMouseEnter()
    {
        if ((transform.position - GameManager.Instance.ownerPlayer.transform.position).sqrMagnitude < range)
        {
            foreach (var mesh in meshRenderers)
            {
                Material[] materials = mesh.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == outlineMat) // match by reference
                    {
                        materials[i].SetFloat("_Scale", glowScale);
                    }
                }

                mesh.materials = materials; // Re-assign the modified array
            }
        }
    }
    
    void OnMouseExit()
    {
        if ((transform.position - GameManager.Instance.ownerPlayer.transform.position).sqrMagnitude < range)
        {
            foreach (var mesh in meshRenderers)
            {
                Material[] materials = mesh.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == outlineMat) // match by reference
                    {
                        materials[i].SetFloat("_Scale", 0);
                    }
                }

                mesh.materials = materials; // Re-assign the modified array
            }
        }
    }
    #endregion
}
