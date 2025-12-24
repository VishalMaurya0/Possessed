using Unity.Netcode;


[System.Serializable]
public class ItemData : INetworkSerializable
{
    public ItemType itemType;
    public int currentState;
    public int amount;
    public bool isOn;
    public int photoType = 0; // 0 = normal, 1 = procedure, 2 = statue
    public int photoId = 0;

    public ItemData(ItemDataSO idSO, int amount, int CurrentState, int photoType, int photoID)
    {
        itemType = idSO.itemType;
        currentState = CurrentState;
        this.amount = amount;
        this.photoType = photoType;
        this.photoId = photoID;
    }

    public ItemData(ItemData itemData)
    {
        itemType = itemData.itemType;
        currentState = itemData.currentState;
        amount = itemData.amount;
        photoType = itemData.photoType;
        photoId = itemData.photoId;
        isOn = itemData.isOn;
    }
    
    public ItemData(int amount, int CurrentState, int photoType, int photoID) 
    {
        this.itemType = ItemType.Photo;
        currentState = CurrentState;
        this.amount = amount;
        this.photoType = photoType;
        this.photoId = photoID;
    }

    public ItemData() { }            //======================Default Constructor For serialization Method to Work========================//
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref currentState);
        serializer.SerializeValue(ref amount);
        serializer.SerializeValue(ref itemType);
        serializer.SerializeValue(ref isOn);
        serializer.SerializeValue(ref photoType);
        serializer.SerializeValue(ref photoId);
    }
}

[System.Serializable]
public class PhotoData : INetworkSerializable
{
    public int photoID;
    public bool ProcedurePhoto;
    public bool StatuePhoto;
    public PhotoData(int photoID, bool procedurePhoto, bool statuePhoto)
    {
        this.photoID = photoID;
        ProcedurePhoto = procedurePhoto;
        StatuePhoto = statuePhoto;
    }
    public PhotoData() { }            //======================Default Constructor For serialization Method to Work========================//
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref photoID);
        serializer.SerializeValue(ref ProcedurePhoto);
        serializer.SerializeValue(ref StatuePhoto);
    }
}




public enum ItemType
{
    //------Items To Collect-----//
    Wood,
    Match,
    BloodBottle,
    Mirror,
    PurePowder,
    Candle,
    CursedCoins,
    Feather,
    Cloth,
    VoodooDoll,
    Pin,

    //------Items Given to find Procedures------//
    EnergyDetector,
    EMFReader,
    Thermometer,
    Barometer,

    //------Items Given At Start-------//
    Torch,
    ItemDuplicator,
    SafePoint,
    PhotoAlbum,

    //------Photos-------//
    Photo,

    //------Items To Craft-----//
}


