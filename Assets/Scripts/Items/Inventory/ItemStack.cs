

using Data;

namespace Items
{
  public record ItemStack
  {
    public ItemData data;
    public short amount;
    public int durability;
  }
}
