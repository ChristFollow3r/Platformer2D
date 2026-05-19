public static class BlockIdUtils
{
    /// <summary>Packs a (x, y) cell into a unique ulong block ID.</summary>
    public static ulong From(int x, int y)
    {
        ulong ux = (uint)x; // preserve negatives via reinterpret
        ulong uy = (uint)y;
        return (ux << 32) | uy;
    }

    /// <summary>Extracts x and y back from a block ID.</summary>
    public static (int x, int y) ToCell(ulong id)
    {
        int x = (int)(uint)(id >> 32);
        int y = (int)(uint)(id & 0xFFFFFFFF);
        return (x, y);
    }
}
