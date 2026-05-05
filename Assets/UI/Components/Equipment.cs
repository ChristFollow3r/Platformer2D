
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Equipment : VisualElement
  {

    #region Data
    private Slot[] equipmentSlots = new Slot[Items.Overlays.Equipment.EquipmentSlots];
    private Slot[] craftingSlots = new Slot[4];
    private Slot resultSlot;
    #endregion

    #region Backers
    #endregion

    #region Elements
    private VisualElement rootElm;
    private VisualElement equipmentList;
    private VisualElement craftingHolder;
    private VisualElement craftingGrid;
    #endregion

    #region Constructor
    public Equipment()
    {
      VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Components/Equipment/Equipment");
      tree.CloneTree(this);

      GetElements();
      CreateSlots();
    }
    #endregion

    #region Methods
    private void GetElements()
    {
      #region GetElements
      rootElm = this.Q<VisualElement>("root");
      equipmentList = this.Q<VisualElement>("equipment");
      craftingHolder = this.Q<VisualElement>("crafting-holder");
      craftingGrid = this.Q<VisualElement>("crafting-grid");
      #endregion
    }

    private void CreateSlots()
    {
      #region CreateSlots
      for (short i = 0; i < equipmentSlots.Length; i++)
      {
        Slot slot = new Slot();

        equipmentList.Add(slot);
        equipmentSlots[i] = slot;
        slot.slotId = i;
      }

      for (short i = 0; i < craftingSlots.Length; i++)
      {
        Slot slot = new Slot();

        int col = i % 2;
        int row = i / 2;
        if (col < 1) slot.AddToClassList("spaced-right");
        if (row < 1) slot.AddToClassList("spaced-bottom");

        craftingGrid.Add(slot);
        craftingSlots[i] = slot;
        slot.slotId = (short)(equipmentSlots.Length + i);
      }

      resultSlot = new Slot();
      craftingHolder.Add(resultSlot);
      #endregion
    }
    #endregion
  }
}
