using System.Collections.Generic;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Data
{
    public enum BlockType
    {
        None,
        Air,
        Dirt,
        Grass,
        Stone,
        SurfaceSand,
        Sand,
        SurfaceClay,
        Clay,
        Gravel,
        Slate,
        Coal,
        Copper,
        Tin,
        Iron,
        Sapphire,
        Topaz,
        Emerald,
        Onyx,
        Ruby,
        Bedrock,
        WoodPlank,
        StoneBrick,
        SlateBrick,
        SandBrick,
        CraftingTable,
        Furnace,
        Chest,
        SaphireBlock,
        TopazBlock,
        EmeraldBlock,
        OnyxBlock,
        RubyBlock,
        CopperBlock,
        BronzeBlock,
        IronBlock,
        spawnAnchor
    } // Added none for the items

    public enum PropType { None, Bush, StoneProp, OakTree, BirchTree, Copper, Iron, Coal, Sulphur, ScareCrow }

    public enum Hardness { Level1, Level2, Level3, Level4, Level5 }

    public static class WorldData
    {
        public static readonly Dictionary<BlockType, ItemData> BlockDictionary = new();
        public static readonly Dictionary<PropType, Prop> PropDictionary = new();
        public static World World;

        public static bool isGenerating = false;
        public static readonly HashSet<Vector2Int> dirtyBlocks = new();
        public static readonly HashSet<Vector2Int> dirtyProps = new();
    }
}
