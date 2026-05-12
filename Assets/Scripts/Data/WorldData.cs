using System.Collections.Generic;
using Scriptable_Objects_Scripts;

namespace Data
{
    public enum BlockType { None, Air, Dirt, Grass, Stone, Sand, Coal, Copper, Tin, Iron, Sapphire, Topaz, Emerald, Onyx, Ruby} // Added none for the items

    public enum PropType { None, Bush, StoneProp, OakTree, BirchTree, Copper, Iron, Coal, Sulphur, ScareCrow }

    public enum Hardness { Level1, Level2, Level3, Level4, Level5 }

    public static class WorldData
    {
        public static readonly Dictionary<BlockType, Block> BlockDictionary = new();
        public static readonly Dictionary<PropType, Prop> PropDictionary = new();
        public static World World;
    }
}
