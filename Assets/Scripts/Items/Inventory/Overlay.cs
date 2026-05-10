using Player;
using Unity.IO.LowLevel.Unsafe;

namespace Items
{

  public abstract class Overlay
  {
    #region Data
    public int blockId;
    protected bool isOpen;
    #endregion

    #region Constructor
    public Overlay(int blockId)
    {
      this.blockId = blockId;
      UIController.Singleton.OnOverlayOpen += OnOverlayOpen;
    }
    #endregion

    #region Methods
    private void OnOverlayOpen(int blockId, OverlayType overlayType, object data)
    {
      if (blockId != this.blockId) return;
      isOpen = true;
      UIController.Singleton.OnOverlayClose += OnOverlayClose;
    }
    private void OnOverlayClose()
    {
      UIController.Singleton.OnOverlayClose -= OnOverlayClose;
      CloseOverlay();
      isOpen = false;
    }

    protected virtual void CloseOverlay() { }

    public virtual void RefreshUI() { }
    #endregion
  }
}
