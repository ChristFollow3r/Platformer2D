

using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class World : VisualElement
    {
        #region Data
        [UxmlAttribute] public string worldName { get => _worldName; set => SetWorldName(value); }

        private string _worldName;

        private Label worldNameElm;
        private Button loadBtn;
        #endregion
        #region Setters
        private void SetWorldName(string worldName)
        {
            _worldName = worldName;
            worldNameElm.text = _worldName;
        }
        #endregion

        #region Constructor
        public World()
        {
            worldNameElm = this.Q<Label>("name");
            loadBtn = this.Q<Button>("Load");

            loadBtn.clicked += () => { }; // TODO ADD cb
        }
        #endregion
    }
}
