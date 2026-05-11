using Data;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

public class T : MonoBehaviour
{

  public ItemData itemData;
  public ItemData itemData2;

  public

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

    Inventory.Singleton.AddToSlot(testStack3, 15);
    Inventory.Singleton.Add(testStack);
    Inventory.Singleton.Add(testStack2);
  }

  // Update is called once per frame
  void Update()
  {

  }
}
