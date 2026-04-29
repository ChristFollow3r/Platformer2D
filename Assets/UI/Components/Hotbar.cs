
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class Hotbar : VisualElement
    {

        #region Data
        [UxmlAttribute] public bool hasBackground { get; set; }
        #endregion

        #region Backers

        #endregion

        #region Elements
        private VisualElement rootElm;
        private VisualElement backgroundElm;
        #endregion

        #region Constructor
        public Hotbar()
        {
            VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Hotbar/Hotbar");
            tree.CloneTree(this);

            GetElements();
            CreateElements();
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
            }
            #endregion
        }
        #endregion
    }
}