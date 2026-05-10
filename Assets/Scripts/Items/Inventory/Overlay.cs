using Player;
using Unity.IO.LowLevel.Unsafe;

namespace Items
{

  public abstract class Overlay
  {
    #region Data
    public int blockId;
    #endregion

    #region Constructor
    public Overlay(int blockId)
    {
      this.blockId = blockId;

      UIController.Singleton.OnOverlayOpen += OnOverlayOpen;
      UIController.Singleton.OnOverlayClose += OnOverlayClose;

    }
    #endregion

    #region Methods
    private void OnOverlayOpen(OverlayType overlayType, object data) { }
    private void OnOverlayClose()
    {
      UIController.Singleton.OnOverlayOpen -= OnOverlayOpen;
      UIController.Singleton.OnOverlayClose -= OnOverlayClose;
      CloseOverlay();
    }

    protected virtual void CloseOverlay() { }
    #endregion
  }
}
