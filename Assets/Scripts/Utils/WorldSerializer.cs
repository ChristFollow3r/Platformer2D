
using System;
using System.IO;
using System.Linq;
using Data;
using UnityEngine;

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

[Serializable]
public class WorldSaveData
{
    public string seed;
    public BlockDiff[] blocks;
    public PropDiff[] props;
}
[Serializable]
public class BlockDiff { public int x, y; public BlockType type; }

[Serializable]
public class PropDiff { public int x, y; public PropType type; }

static class WorldSerializer
{
    public static string currentSavePath;
    private static string SavePath => currentSavePath ?? Application.persistentDataPath + "/world.json";

    public static void Save(string seed)
    {
        var data = new WorldSaveData
        {
            seed = seed,
            blocks = WorldData.dirtyBlocks
                .Select(p => new BlockDiff { x = p.x, y = p.y, type = WorldData.World.GetBlockTypes(p.x, p.y) })
                .ToArray(),
            props = WorldData.dirtyProps
                .Select(p => new PropDiff { x = p.x, y = p.y, type = WorldData.World.GetPropType(p.x, p.y) })
                .ToArray()
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data));
    }

    public static WorldSaveData Load()
    {
        if (!File.Exists(SavePath)) return null;
        return JsonUtility.FromJson<WorldSaveData>(File.ReadAllText(SavePath));
    }

    public static bool Exists() => File.Exists(SavePath);

    public static void Delete()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
