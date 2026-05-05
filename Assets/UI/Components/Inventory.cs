
using Items;
using Scriptable_Objects_Scripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Inventory : VisualElement
  {

    #region Data
    private Slot[] slots = new Slot[Items.Inventory.Cols * Items.Inventory.Rows];
    #endregion

    #region Backers
    #endregion

    #region Elements
    private VisualElement rootElm;
    #endregion

    #region Constructor
    public Inventory()
    {
      VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Inventory/Inventory");
      tree.CloneTree(this);

      GetElements();
      SubscribeEvents();
    }
    #endregion

    #region Setters
    #endregion

    #region Methods
    private void GetElements()
    {
      #region GetElements
      rootElm = this.Q<VisualElement>("root");

      for (short i = 0; i < slots.Length; i++)
      {
        Slot slot = new Slot();

        int col = i % Items.Inventory.Cols;
        int row = i / Items.Inventory.Cols;

        if (col < Items.Inventory.Cols - 1) slot.AddToClassList("spaced-right");
        if (row < Items.Inventory.Rows - 1) slot.AddToClassList("spaced-bottom");

        rootElm.Add(slot);
        slots[i] = slot;
        slot.slotId = (short)(Items.Inventory.HotbarItems + i);
      }
      #endregion
    }

    private void SubscribeEvents()
    {
      #region SubscribeEvents
      Items.Inventory.OnSlotChanged += OnSlotChange;
      #endregion
    }
    private void OnSlotChange(int slotId, ItemStack item)
    {
      #region OnSlotChange
      if (slotId < Items.Inventory.HotbarItems) return;
      Item itemElm = null;
      if (item is not null)
      {
        itemElm = new Item(true)
        {
          item = item.data,
          amount = item.amount
        };
      }

      slots[slotId - Items.Inventory.HotbarItems].item = itemElm;
      #endregion
    }
    #endregion
  }
}
