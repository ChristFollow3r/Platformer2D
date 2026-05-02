
using Scriptable_Objects_Scripts;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Inventory : VisualElement
  {

    #region Data
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
    }
    #endregion

    #region Setters
    #endregion

    #region Methods
    private void GetElements()
    {
      #region GetElements
      rootElm = this.Q<VisualElement>("root");
      #endregion
    }
    #endregion
  }
}
