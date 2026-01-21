using UnityEngine;

public class MirrorBehaviour : MonoBehaviour
{
    public Camera camera;
    GameObject player; 
    public GameObject forward; 
    public MeshRenderer mirrorSurface;
    public Material referenceMat;
    private RenderTexture runtimeRT;
    private Material runtimeMaterial;
    public bool GoAccordingToPlayer = true;
    private void Start()
    {
        camera = GameObject.FindWithTag("Ghost").GetComponentInChildren<Camera>();


        //runtimeRT = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
        //runtimeRT.Create();

        // Step 2: Assign to mirror camera
        //if (camera != null)
        //{
        //    camera.targetTexture = runtimeRT;
        //}

        //runtimeMaterial = new Material(referenceMat);
        //runtimeMaterial.mainTexture = runtimeRT;

        // Step 4: Apply material to the mirror surface
        //if (mirrorSurface != null)
        //{
        //    mirrorSurface.material = runtimeMaterial;
        //}
    }

    private void Update()
    {
        if (!GoAccordingToPlayer) return;
        if (player == null)
        {
            player = GameManager.Instance.ownerPlayer;
            return;
        }

        Transform playerCam = player.transform.GetChild(0);

        float x = playerCam.transform.position.x - transform.position.x;
        float z = playerCam.transform.position.z - transform.position.z;
        float y = playerCam.transform.position.y - transform.position.y;

        Vector3 dir = new(x, y, z);

        float camZ = Vector3.Dot(dir, forward.transform.position - transform.position);
        Vector3 final = dir - (forward.transform.position - transform.position) * camZ * 2;

        camera.transform.localPosition = final;
    }
}
