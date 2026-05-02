using Data;
using Items;
using UnityEngine;

public class T : MonoBehaviour
{

  public ItemData itemData;

  void Start()
  {
    ItemStack testStack = new ItemStack
    {
      data = itemData,
      amount = 4
    };
    Inventory.Add(testStack);
  }

  // Update is called once per frame
  void Update()
  {

  }
}
