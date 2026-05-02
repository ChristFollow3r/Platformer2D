
using Items;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Hotbar : VisualElement
  {

    #region Data
    [UxmlAttribute] public bool isMain { get; set; }

    private Slot[] slots = new Slot[10];
    #endregion

    #region Backers

    #endregion

    #region Elements
    private VisualElement rootElm;
    private VisualElement backgroundElm;
    private Slot currentHand;
    #endregion

    #region Constructor
    public Hotbar()
    {
      VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Hotbar/Hotbar");
      tree.CloneTree(this);

      GetElements();
      CreateElements();
      SubscribeEvents();
      OnHandChanged(0);
    }
    #endregion

    #region Setters
    #endregion

    #region Methods
    private void GetElements()
    {
      #region GetElements
      rootElm = this.Q<VisualElement>("root");
      backgroundElm = this.Q<VisualElement>("background");
      #endregion
    }

    private void CreateElements()
    {
      #region CreateElements
      for (int i = 0; i < 10; i++)
      {
        Slot slot = new Slot();
        if (i != 9) slot.AddToClassList("spaced-right");
        rootElm.Add(slot);
        slots[i] = slot;
      }
      #endregion
    }

    private void SubscribeEvents()
    {
      #region SubscribeEvents
      if (isMain)
      {
        Items.Inventory.OnHandChanged += OnHandChanged;
      }
      Items.Inventory.OnSlotChanged += OnSlotChange;
      #endregion
    }

    private void OnSlotChange(int slotId, ItemStack item)
    {
      #region OnSlotChange
      if (slotId >= Items.Inventory.HotbarItems) return;

      Item newItem = new Item
      {
        item = item.data,
        amount = item.amount
      };

      slots[slotId].item = newItem;
      #endregion
    }

    /// <summary>Method</summary>
    private void OnHandChanged(short handIndex)
    {
      #region OnHandChanged
      if (currentHand != null) currentHand.RemoveFromClassList("hand");
      currentHand = slots[handIndex];
      currentHand.AddToClassList("hand");
      #endregion
    }
    #endregion
  }
}
