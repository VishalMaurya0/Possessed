using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhotoContainerSO", menuName = "Scriptable Objects/PhotoContainerSO")]
public class PhotoContainerSO : ScriptableObject
{
    public List<FullPhotoData> ProcedurePhotos;
    public List<FullPhotoData> StatuePhotos;
    public int maxPhotoOfSelectedProcedure = 8;
    public int secMaxPhotoToStopPuttingPhotos = 5;
    public GameObject photoPrefab;
}

[System.Serializable]
public class FullPhotoData
{
    public string photoName;
    public Sprite photoSprite;
    public string description;
    public PhotoData photoData;
}