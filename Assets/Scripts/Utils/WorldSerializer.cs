
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

[Serializable]
public class TutorialData
{
    public int basicState;
    public int objectivesState;

    public bool b1_wood, b1_resin, b1_table;
    public bool b2_fiber, b2_string, b2_rock, b2_resin, b2_rocks, b2_clay, b2_furnace;
    public bool b3_rocks, b3_smelt;
    public bool b4_upgrade;
    public bool b5_use;
    public bool b6_consume_slime;
    public bool b7_copperOre, b7_magic, b7_copperIngot, b7_copperUpgrade;
    public bool b8_stoneBlock, b8_magic, b8_slime, b8_anchor, b8_setSpawn;
    public bool b9_tinOre, b9_copperOre, b9_bronzeIngot, b9_compressed, b9_bronzeUpgrade;
    public bool b10_ironOre, b10_compressed, b10_greater, b10_ironUpgrade;
    public bool b11_slime, b11_greater, b11_emerald, b11_ruby, b11_topaz, b11_onyx, b11_primordialUpgrade;
    public bool b12_destinyStone, b12_interactDestiny;
}

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
    private static string TutorialPath(string saveName) => Path.Combine(SaveFolder(saveName), "tutorial.json");

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
        if (PlayerMovement.Singleton) File.WriteAllText(PlayerPath(WorldName), PlayerMovement.Singleton.Serialize());
        if (TutorialManager.Instance) File.WriteAllText(TutorialPath(WorldName), TutorialManager.Instance.Serialize());

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

    public static void LoadTutorial()
    {
        string path = TutorialPath(WorldName);
        Debug.Log($"[WorldSerializer] Loading tutorial from: {path}");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[WorldSerializer] Tutorial file not found at: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        Debug.Log($"[WorldSerializer] Tutorial JSON length: {json.Length}");

        TutorialManager.Instance.Deserialize(json);
        Debug.Log("[WorldSerializer] Tutorial deserialized.");
    }
}
