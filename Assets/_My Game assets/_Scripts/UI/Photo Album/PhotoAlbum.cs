using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PhotoAlbum : MonoBehaviour
{
    [Header("References")]
    public Animator Animator;
    public GameObject PhotoContainerPrefab;
    public GameObject PhotoContainerParent;
    public List<Sprite> AllSprites = new ();

    [Header("Properties")]
    public List<GameObject> PhotoContainers = new();
    public List<GameObject> Photos = new();
    public int noOfPhotos;
    public int currentSpriteID;


    private void Start()
    {
        Animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Animator.SetBool("Load", !Animator.GetBool("Load"));
            if (Animator.GetBool("Load"))
            {
                GameManager.Instance.lockCurser = false;
                GameManager.Instance.handlePlayerLookWithMouse = false;
            }
            else
            {
                GameManager.Instance.lockCurser = true;
                GameManager.Instance.handlePlayerLookWithMouse = true;
            }
        }
    }

    public bool AddPhotoInAlbum(Sprite sprite)
    {
        int spriteID = 0;
        for (int i = 0; i < AllSprites.Count; i++)
        {
            if (AllSprites[i] == sprite)
            {
                spriteID = i;
                AddNewPhotoServerRpc(spriteID);
                return true;
            }
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddNewPhotoServerRpc(int spriteID)
    {
        PhotoContainers.Add(Instantiate(PhotoContainerPrefab, PhotoContainerParent.transform));
        int index = PhotoContainers.Count - 1;
        Photos.Add(PhotoContainers[index].GetComponentInChildren<DummyScriptForAccessingElement>().gameObject);
        Photos[index].GetComponent<Image>().sprite = AllSprites[spriteID];
        noOfPhotos++;
    }
}