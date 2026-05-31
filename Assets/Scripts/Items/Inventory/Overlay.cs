using Player;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace Items
{

    public abstract class Overlay
    {
        #region Data
        public ulong blockId;
        protected bool isOpen;
        public OverlayType overlayType;
        #endregion

        #region Constructor
        public Overlay(ulong blockId, OverlayType overlayType)
        {
            this.blockId = blockId;
            this.overlayType = overlayType;
            UIController.Singleton.OnOverlayOpen += OnOverlayOpen;
        }
        #endregion

        #region Methods
        private void OnOverlayOpen(ulong blockId, OverlayType overlayType)
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

        public virtual void Tick() { }

        protected virtual void CloseOverlay() { }

        public virtual void RefreshUI() { }
        #endregion
    }
}
