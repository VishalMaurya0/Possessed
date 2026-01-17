using System;
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
    public PhotoContainerSO PhotoContainerSO;

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
                AudioManager.PlaySound(AudioType.PanelOpen);
            }
            else
            {
                GameManager.Instance.lockCurser = true;
                GameManager.Instance.handlePlayerLookWithMouse = true;
                AudioManager.PlaySound(AudioType.PanelClose);
            }
        }
    }

    //public bool AddPhotoInAlbum(Sprite sprite)
    //{
    //    int spriteID = 0;
    //    for (int i = 0; i < AllSprites.Count; i++)
    //    {
    //        if (AllSprites[i] == sprite)
    //        {
    //            spriteID = i;
    //            AddNewPhoto(spriteID);
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //[ServerRpc(RequireOwnership = false)]
    private void AddNewPhoto(int spriteID)
    {
        PhotoContainers.Add(Instantiate(PhotoContainerPrefab, PhotoContainerParent.transform));
        int index = PhotoContainers.Count - 1;
        PhotoContainers[index].name = "PhotoContainer_" + (PhotoContainers.Count - 1).ToString();
        PhotoContainers[index].SetActive(true);
        Photos.Add(PhotoContainers[index].GetComponentInChildren<DummyScriptForAccessingElement>().gameObject);
        Photos[index].GetComponent<Image>().sprite = AllSprites[spriteID];
        noOfPhotos++;
    }

    internal void UpdatePhotoAlbumUI()
    {
        if (noOfPhotos != GameManager.Instance.collectedPhotos.Count)
        {
            //for (int i = noOfPhotos; i < GameManager.Instance.collectedPhotos.Count; i++)
            {
                Sprite sprite = null;

                if (GameManager.Instance.collectedPhotos[GameManager.Instance.collectedPhotos.Count - 1].ProcedurePhoto)
                {
                    for (int j = 0; j < PhotoContainerSO.ProcedurePhotos.Count; j++)
                    {
                        if (PhotoContainerSO.ProcedurePhotos[j].photoData.photoID == GameManager.Instance.collectedPhotos[GameManager.Instance.collectedPhotos.Count - 1].photoID)
                        {
                            sprite = PhotoContainerSO.ProcedurePhotos[j].photoSprite;
                            break;
                        }
                    }
                }else if (GameManager.Instance.collectedPhotos[GameManager.Instance.collectedPhotos.Count - 1].StatuePhoto)
                {
                    for (int j = 0; j < PhotoContainerSO.StatuePhotos.Count; j++)
                    {
                        if (PhotoContainerSO.StatuePhotos[j].photoData.photoID == GameManager.Instance.collectedPhotos[GameManager.Instance.collectedPhotos.Count - 1].photoID)
                        {
                            sprite = PhotoContainerSO.StatuePhotos[j].photoSprite;
                            break;
                        }
                    }
                }

                if (sprite == null)
                {
                    Debug.LogError("Sprite not found for the collected photo.");
                    return;
                }
                AllSprites.Add(sprite);
                AddNewPhoto(AllSprites.Count - 1);
            }
        }
    }
}