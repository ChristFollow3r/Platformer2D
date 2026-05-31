
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
    public float duration;
}

[Serializable]
public class SaveFile
{
    public OverlaySaveEntry[] overlays;
}

[Serializable]
public class OverlaySaveEntry
{
    public string type;
    public ulong blockId;
    public string data;
}

[Serializable] public class ChestData { public SlotData[] slots; }
[Serializable] public class EquipmentData { public SlotData[] equipmentSlots; }
[Serializable] public class FurnaceData { public SlotData[] cookingSlots; public SlotData fuelSlot; }

static class WorldSerializer
{

}
