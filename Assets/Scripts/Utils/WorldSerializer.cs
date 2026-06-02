
using System;
using System.IO;
using System.Linq;
using Data;
using Items;
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
    public SlotData(Slot slot)
    {
        id = slot.id;
        if (slot.isEmpty) return;

        itemId = slot.item.data.name;
        amount = slot.item.amount;
        duration = slot.item.duration;
    }
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
[Serializable] public class FurnaceData { public SlotData[] cookingSlots; public SlotData fuelSlot; public SlotData resultSlot; }

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


[Serializable]
public class PlayerSaveData { public Vector2 pos; public int health; public Vector3 spawnPoint; }

static class WorldSerializer
{
    public static bool isNewWorld;
    public static string WorldName;
    public static string Seed;

    private static string SaveFolder(string saveName) =>
        Path.Combine(Application.persistentDataPath, saveName);

    private static string WorldPath(string saveName) => Path.Combine(SaveFolder(saveName), "world.json");
    private static string OverlaysPath(string saveName) => Path.Combine(SaveFolder(saveName), "overlays.json");
    private static string PlayerPath(string saveName) => Path.Combine(SaveFolder(saveName), "player.json");

    public static bool Exists(string saveName) => File.Exists(WorldPath(saveName));

    public static void Delete(string saveName)
    {
        if (Directory.Exists(SaveFolder(saveName)))
            Directory.Delete(SaveFolder(saveName), recursive: true);
    }

    public static void Save()
    {
        Directory.CreateDirectory(SaveFolder(WorldName));

        var data = new WorldSaveData
        {
            seed = Seed,
            blocks = WorldData.dirtyBlocks
                .Select(p => new BlockDiff { x = p.x, y = p.y, type = WorldData.World.GetBlockTypes(p.x, p.y) })
                .ToArray(),
            props = WorldData.dirtyProps
                .Select(p => new PropDiff { x = p.x, y = p.y, type = WorldData.World.GetPropType(p.x, p.y) })
                .ToArray()
        };

        File.WriteAllText(WorldPath(WorldName), JsonUtility.ToJson(data));
        File.WriteAllText(OverlaysPath(WorldName), UIController.Singleton.SerializeAll());
        if (!PlayerMovement.Singleton) return;
        File.WriteAllText(PlayerPath(WorldName), PlayerMovement.Singleton.Serialize());
    }

    public static WorldSaveData Load()
    {
        string path = WorldPath(WorldName);
        Debug.Log($"[WorldSerializer] Loading world from: {path}");

        if (!File.Exists(path))
        {
            Debug.LogError($"[WorldSerializer] World file not found at: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        Debug.Log($"[WorldSerializer] World JSON length: {json.Length}");

        WorldSaveData save = JsonUtility.FromJson<WorldSaveData>(json);

        if (save == null)
        {
            Debug.LogError("[WorldSerializer] Failed to deserialize world JSON.");
            return null;
        }
        Seed = save.seed;

        Debug.Log($"[WorldSerializer] Loaded world. Seed: '{save.seed}', Blocks: {save.blocks?.Length ?? 0}, Props: {save.props?.Length ?? 0}");
        return save;
    }

    public static void LoadOverlays()
    {
        string path = OverlaysPath(WorldName);
        Debug.Log($"[WorldSerializer] Loading overlays from: {path}");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[WorldSerializer] Overlays file not found at: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        Debug.Log($"[WorldSerializer] Overlays JSON length: {json.Length}");

        UIController.Singleton.DeserializeAll(json);
        Debug.Log("[WorldSerializer] Overlays deserialized.");
    }

    public static void LoadPlayer()
    {
        string path = PlayerPath(WorldName);
        Debug.Log($"[WorldSerializer] Loading player from: {path}");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[WorldSerializer] Player file not found at: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        Debug.Log($"[WorldSerializer] Player JSON length: {json.Length}");

        PlayerMovement.Singleton.Deserialize(json);
        Debug.Log("[WorldSerializer] Player deserialized.");
    }
}
