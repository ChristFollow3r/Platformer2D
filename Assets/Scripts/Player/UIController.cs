using UnityEngine;
using UnityEngine.UIElements;


namespace Player
{
  public enum OverlayType
  {
    Inventory
  };


  public class UIController : MonoBehaviour
  {
    #region Data
    public InputSystem_Actions playerInput;
    public UIDocument overlay;
    public UIDocument hud;

    private bool isOverlayOpen;
    private bool canOpenInventory;
    #endregion

    #region Unity
    /// <summary>Ran by unity on load</summary>
    private void Awake()
    {
      #region Awake
      playerInput = new InputSystem_Actions();
      playerInput.Enable();
      #endregion
    }
    /// <summary>Ran by unity each frame</summary>
    private void Update()
    {
      #region Update
      CheckOverlay();
      #endregion
    }
    #endregion

    #region Methods
    private void CheckOverlay()
    {
      #region CheckOverlay
      if (playerInput.UI.OpenInventory.WasPressedThisFrame())
      {
        isOverlayOpen = !isOverlayOpen;

        if (isOverlayOpen) OpenOverlay(OverlayType.Inventory);
        else
        {
          overlay.enabled = false;
          hud.enabled = true;
        }
        return;
      }

      if (playerInput.UI.CloseOverlay.WasPressedThisFrame() && isOverlayOpen)
      {
        overlay.enabled = false;
        hud.enabled = true;
        isOverlayOpen = false;
      }
      #endregion
    }
    #endregion


    private void OpenOverlay(OverlayType overlayType)
    {
      #region OpenOverlay
      overlay.enabled = false;
      // TODO: set type of overlay for ui
      hud.enabled = true;
      #endregion
    }
  }
}
