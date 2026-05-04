using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Hotbar : VisualElement
  {

    #region Data
    [UxmlAttribute] public bool isMain { get => _isMain; set => SetIsMain(value); }
    private Slot[] slots = new Slot[Items.Inventory.HotbarItems];
    #endregion

    #region Backers
    private bool _isMain = false;
    #endregion

    #region Elements
    private VisualElement rootElm;
    private VisualElement backgroundElm;
    private Slot currentHand;
    #endregion

    #region Constructor
    public Hotbar()
    {
      VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Components/Hotbar/Hotbar");
      tree.CloneTree(this);

      GetElements();
      CreateElements();
      SubscribeEvents();
      if (isMain) OnHandChanged(0);
    }
    #endregion

    #region Setters
    private void SetIsMain(bool isMain)
    {
      #region SetIsMain
      _isMain = isMain;
      if (isMain) backgroundElm.AddToClassList("hotbar-background-show");
      else backgroundElm.RemoveFromClassList("hotbar-background-show");

      #endregion
    }
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
      for (int i = 0; i < slots.Length; i++)
      {
        Slot slot = new Slot();
        if (i != slots.Length - 1) slot.AddToClassList("spaced-right");
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
      Item itemElm = null;
      if (item is not null)
      {
        itemElm = new Item(!isMain)
        {
          item = item.data,
          amount = item.amount
        };
      }

      slots[slotId].item = itemElm;
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
