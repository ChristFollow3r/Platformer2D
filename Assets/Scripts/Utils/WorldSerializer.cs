
using System;
using System.IO;
using System.Linq;
using Data;
using Player;
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
    public static string WorldName;

    private static string SaveFolder(string saveName) =>
        Path.Combine(Application.persistentDataPath, saveName);

    private static string WorldPath(string saveName) => Path.Combine(SaveFolder(saveName), "world.json");
    private static string OverlaysPath(string saveName) => Path.Combine(SaveFolder(saveName), "overlays.json");

    public static bool Exists(string saveName) => File.Exists(WorldPath(saveName));

    public static void Delete(string saveName)
    {
        if (Directory.Exists(SaveFolder(saveName)))
            Directory.Delete(SaveFolder(saveName), recursive: true);
    }

    public static void Save(string saveName, string seed)
    {
        Directory.CreateDirectory(SaveFolder(saveName));

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

        File.WriteAllText(WorldPath(saveName), JsonUtility.ToJson(data));
        File.WriteAllText(OverlaysPath(saveName), UIController.Singleton.SerializeAll());
    }

    public static WorldSaveData Load()
    {
        if (!File.Exists(WorldPath(WorldName))) return null;
        return JsonUtility.FromJson<WorldSaveData>(File.ReadAllText(WorldPath(WorldName)));
    }

    public static void LoadOverlays()
    {
        if (!File.Exists(OverlaysPath(WorldName))) return;
        UIController.Singleton.DeserializeAll(File.ReadAllText(OverlaysPath(WorldName)));
    }
}
