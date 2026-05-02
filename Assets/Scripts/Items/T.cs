using Data;
using Items;
using UnityEngine;

public class T : MonoBehaviour
{

  public ItemData itemData;
  public ItemData itemData2;

  void Start()
  {
    ItemStack testStack = new ItemStack
    {
      data = itemData,
      amount = 4
    };
    ItemStack testStack3 = new ItemStack
    {
      data = itemData,
      amount = 4
    };
    ItemStack testStack2 = new ItemStack
    {
      data = itemData2,
      amount = 10
    };
    Inventory.Add(testStack);
    Inventory.Add(testStack3);
    Inventory.Add(testStack2);
  }

  // Update is called once per frame
  void Update()
  {

  }
}
