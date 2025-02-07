using Unity.Netcode;


[System.Serializable]
public class ItemData : INetworkSerializable
{
    public ItemType itemType;
    public int currentState;
    public int amount;
    public bool isOn;

    public ItemData(ItemDataSO idSO, int amount, int CurrentState) 
    {
        itemType = idSO.itemType;
        currentState = CurrentState;
        this.amount = amount;
    }

    public ItemData() { }            //======================Default Constructor For serialization Method to Work========================//
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref currentState);
        serializer.SerializeValue(ref amount);
        serializer.SerializeValue(ref itemType);
        serializer.SerializeValue(ref isOn);
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
    EMFSenser,
    GC,
    Camera,
    Barometer,

    //------Items Given At Start-------//
    Torch,
    ItemDuplicator,
    SafePoint

    //------Items To Craft-----//
}


