
using System;

[Serializable]
public class InventoryData
{
    public SlotData[] slots;
}

[Serializable]
public class SlotData
{
    public int id;
    public string itemId;
    public short amount;
}

static class WorldSerializer
{


}
