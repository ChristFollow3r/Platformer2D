using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Data;
using UnityEngine.InputSystem;
using Shared;

namespace Player
{
    public enum OverlayType
    {
        Inventory,
        Furnace,
        Chest,
        CraftingTable
    };

    [DefaultExecutionOrder(-50)]
    public class UIController : MonoBehaviour
    {
        #region Singleton setup

        public static UIController Singleton;

        private void SetupSingleton()
        {
            #region SetupSingleton

            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;
            #endregion
        }

        #endregion

        #region Data

        [Header("Menu Elements")]
        [SerializeField] private UIDocument pauseMenu;
        [SerializeField] private UIDocument thankScreen;
        public bool isMenuOpen = false;

        [Header("Elements")]
        [SerializeField] private UIDocument overlay;
        [SerializeField] private UIDocument hud;
        [SerializeField] private Font fontPixel;
        private VisualElement nameShower;
        private VisualElement healthElm;
        private VisualElement modElm;

        private VisualElement overlayRoot;
        private VisualElement hudRoot;

        private Dictionary<ulong, Overlay> overlaysByBlockId = new();

        [Header("Controls")] public bool isOverlayOpen;
        private InputSystem_Actions playerInput;

        [Header("Audio")][SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip defaultClickSound;
        [SerializeField] private AudioClip pageTurnSound;

        [Header("Audio Mixing")]
        [SerializeField]
        private AudioMixer mainAudioMixer;

        [Header("Controls Panel")] private VisualElement controlsPanel;
        private Button toggleControlsBtn;

        [Header("Settings Panel")] private VisualElement settingsContainer;
        private Button closeSettingsBtn;

        private Label itemNameElm;

        [SerializeField] private RuntimeAnimatorController defaultController;

        [Header("JEI")]
        private bool isBookOpen = false;
        private VisualElement recipeBookContainer;

        #endregion

        #region Events

        public event Action<ulong, OverlayType> OnOverlayOpen;
        public event Action OnOverlayClose;

        #endregion

        #region Unity

        private void Awake()
        {
            #region Awake

            SetupSingleton();
            playerInput = new InputSystem_Actions();
            playerInput.Enable();
            CreateUI();
            SetupBook();
            #endregion
        }

        private void Start()
        {
            #region Start

            CloseOverlay();
            playerInput.UI.Hand.performed += OnHandNumber;
            #endregion
        }

        private void Update()
        {
            #region Update
            CheckOverlay();
            MoveHand();
            Consume();
            Drop();
            foreach (Overlay overlay in overlaysByBlockId.Values)
            {
                overlay.Tick();
            }

            #endregion
        }

        private void OnDestroy()
        {
            #region OnDestroy

            if (Singleton == this)
            {
                Singleton = null;
            }

            if (playerInput != null)
            {
                playerInput.Disable();
            }

            #endregion
        }
        #endregion

        #region Methods

        private void CreateUI()
        {
            #region CreateUI

            UI.Components.Inventory inventory = new UI.Components.Inventory();
            Hotbar overlayHotbar = new Hotbar { isMain = false };

            overlayRoot = overlay.rootVisualElement;

            overlayRoot.Q("inventory").Add(inventory);
            overlayRoot.Q("holder").Add(overlayHotbar);

            nameShower = overlayRoot.Q("name-holder");

            Hotbar hudHotbar = new Hotbar { isMain = true };
            hud.rootVisualElement.Q("hotbar-holder").Add(hudHotbar);
            CreateOverlay(ulong.MinValue, OverlayType.Inventory);

            pauseMenu.rootVisualElement.style.display = DisplayStyle.None;

            var menuRoot = pauseMenu.rootVisualElement;

            menuRoot.Query<Button>().ForEach(btn =>
            {
                if (btn.name != "PrevPage" && btn.name != "NextPage")
                {
                    btn.clicked += () => PlaySound(defaultClickSound);
                }
            });

            InitializeControls(menuRoot);
            InitializeSettings(menuRoot);

            Button openMenuBtn = hud.rootVisualElement.Q<Button>("OpenMenu");
            if (openMenuBtn != null)
            {
                openMenuBtn.clicked += ToggleMenu;
                openMenuBtn.clicked += () => PlaySound(defaultClickSound);
            }

            Button backBtn = menuRoot.Q<Button>("Back");
            if (backBtn != null) backBtn.clicked += ToggleMenu;

            Button settingsBtn = menuRoot.Q<Button>("Settings");
            if (settingsBtn != null) settingsBtn.clicked += OpenSettings;


            Button saveBtn = menuRoot.Q<Button>("Save");
            if (saveBtn != null) saveBtn.clicked += () => Debug.Log("Save");

            Button exitBtn = menuRoot.Q<Button>("Exit");
            if (exitBtn != null) exitBtn.clicked += () =>
            {
                Time.timeScale = 1f;
                WorldSerializer.Save();
                SceneManager.LoadScene("Menu");
            };

            healthElm = hud.rootVisualElement.Q("health");
            modElm = hud.rootVisualElement.Q("mod");
            itemNameElm = hud.rootVisualElement.Q<Label>("itemName");
            thankScreen.rootVisualElement.Q<Button>("continue").clicked += RemoveThankyouScreen;

            recipeBookContainer = overlayRoot.Q("recipeBook");
            #endregion
        }

        public void ShowThankYouScreen()
        {
            Time.timeScale = 0f;
            CloseOverlay();
            thankScreen.rootVisualElement.Q("root").style.display = DisplayStyle.Flex;
            hud.rootVisualElement.style.display = DisplayStyle.None;
        }

        public void RemoveThankyouScreen()
        {
            Time.timeScale = 1f;
            thankScreen.rootVisualElement.Q("root").style.display = DisplayStyle.None;
            pauseMenu.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        public void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;

            if (isMenuOpen)
            {
                Time.timeScale = 0f;
                pauseMenu.rootVisualElement.style.display = DisplayStyle.Flex;
                hud.rootVisualElement.style.display = DisplayStyle.None;
            }
            else
            {
                Time.timeScale = 1f;
                pauseMenu.rootVisualElement.style.display = DisplayStyle.None;
                hud.rootVisualElement.style.display = DisplayStyle.Flex;

                CloseSettings();
            }
        }

        private void CheckOverlay()
        {
            #region CheckOverlay

            if (playerInput.UI.OpenInventory.WasPressedThisFrame())
            {
                isOverlayOpen = !isOverlayOpen;

                if (isOverlayOpen) OpenOverlay(ulong.MinValue);
                else CloseOverlay();
                return;
            }

            if (playerInput.UI.CloseOverlay.WasPressedThisFrame())
            {
                if (isOverlayOpen) CloseOverlay();
                else ToggleMenu();
            }

            #endregion
        }

        public void OpenOverlay(ulong blockId)
        {
            #region OpenOverlay
            hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
            overlayRoot.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
            isOverlayOpen = true;

            if (!overlaysByBlockId.TryGetValue(blockId, out Overlay foundOverlay)) return;

            VisualElement overlayElement = CreateElement(foundOverlay);
            VisualElement left = overlayRoot.Q("left");
            if (left.childCount != 0) left.RemoveAt(0);
            left.Add(overlayElement);

            OnOverlayOpen?.Invoke(blockId, foundOverlay.overlayType);
            foundOverlay.RefreshUI();

            #endregion
        }

        public void CloseOverlay()
        {
            #region CloseOverlay
            OnOverlayClose?.Invoke();
            hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
            overlayRoot.Q<VisualElement>("root").style.display = DisplayStyle.None;
            isOverlayOpen = false;
            #endregion
        }

        private VisualElement CreateElement(Overlay foundOverlay)
        {
            #region CreateElement

            switch (foundOverlay.overlayType)
            {
                case OverlayType.Inventory:
                    {
                        Items.Overlays.Equipment data = (Items.Overlays.Equipment)foundOverlay;
                        VisualElement element = new Equipment(data);
                        return element;
                    }

                case OverlayType.Furnace:
                    {
                        Items.Overlays.Furnace data = (Items.Overlays.Furnace)foundOverlay;
                        VisualElement element = new Furnace(data);
                        return element;
                    }
                case OverlayType.Chest:
                    {
                        Items.Overlays.Chest data = (Items.Overlays.Chest)foundOverlay;
                        VisualElement element = new Chest(data);
                        return element;
                    }
                case OverlayType.CraftingTable:
                    {
                        Items.Overlays.CraftingTable data = (Items.Overlays.CraftingTable)foundOverlay;
                        VisualElement element = new CraftingTable(data);
                        return element;
                    }
                default:
                    return null;
            }

            #endregion
        }

        public void DestroyEntity(ulong blockId)
        {
            #region DestroyEntity
            overlaysByBlockId[blockId].OnBlockDestroyed();
            overlaysByBlockId.Remove(blockId);
            #endregion
        }

        public Overlay CreateOverlay(ulong blockId, OverlayType overlayType)
        {
            #region CreateOverlay

            switch (overlayType)
            {
                case OverlayType.Inventory:
                    {
                        Items.Overlays.Equipment data = new Items.Overlays.Equipment(defaultController);
                        overlaysByBlockId[blockId] = data;
                        return data;
                    }

                case OverlayType.Furnace:
                    {
                        Items.Overlays.Furnace data = new Items.Overlays.Furnace(blockId);
                        overlaysByBlockId[blockId] = data;
                        return data;
                    }

                case OverlayType.Chest:
                    {
                        Items.Overlays.Chest data = new Items.Overlays.Chest(blockId);
                        overlaysByBlockId[blockId] = data;
                        return data;
                    }

                case OverlayType.CraftingTable:
                    {
                        Items.Overlays.CraftingTable data = new Items.Overlays.CraftingTable(blockId);
                        overlaysByBlockId[blockId] = data;
                        return data;
                    }
                default:
                    return null;
            }

            #endregion
        }

        private void MoveHand()
        {
            #region MoveHand

            Vector2 move = playerInput.UI.MoveHand.ReadValue<Vector2>();
            if (move.y == 0) return;

            if (move.y > 0) Items.Inventory.Singleton.handIndex += 1;
            else Items.Inventory.Singleton.handIndex -= 1;


            #endregion
        }

        private void OnHandNumber(InputAction.CallbackContext ctx)
        {
            string name = ctx.control.name;
            if (!int.TryParse(name, out int digit)) return;
            int slot = (digit == 0) ? 9 : digit - 1;
            if (slot >= Items.Inventory.HotbarSlots) return;

            Items.Inventory.Singleton.handIndex = (short)slot;
        }

        public void ShowName(string name, Vector2 pos, bool isConsumable = false)
        {
            nameShower.style.display = DisplayStyle.Flex;
            Label nameLb = nameShower.Q<Label>("name");

            string iName = name;
            if (isConsumable) iName += " (Consumable)";
            nameLb.text = iName;
            nameShower.style.width = name.Length * 20;
            nameShower.style.left = pos.x;
            nameShower.style.top = pos.y;
        }

        public void HideName()
        {
            nameShower.style.display = DisplayStyle.None;
        }

        private void PlaySound(AudioClip clip)
        {
            if (uiAudioSource != null && clip != null)
            {
                uiAudioSource.PlayOneShot(clip);
            }
        }

        private void InitializeControls(VisualElement menuRoot)
        {
            controlsPanel = menuRoot.Q<VisualElement>("ControlsPanel");
            toggleControlsBtn = menuRoot.Q<Button>("ToggleControlsBtn");

            if (toggleControlsBtn != null)
            {
                toggleControlsBtn.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Settings Logic

        private void InitializeSettings(VisualElement menuRoot)
        {
            settingsContainer = menuRoot.Q<VisualElement>("SettingsContainer");
            if (settingsContainer == null) return;

            settingsContainer.style.position = Position.Absolute;
            settingsContainer.style.width = 800;
            settingsContainer.style.height = 600;
            settingsContainer.style.left = new Length(50, LengthUnit.Percent);
            settingsContainer.style.top = new Length(50, LengthUnit.Percent);
            settingsContainer.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent),
                new Length(-50, LengthUnit.Percent), 0));
            settingsContainer.style.flexDirection = FlexDirection.Row;
            settingsContainer.style.display = DisplayStyle.None;

            VisualElement leftColumn = new VisualElement();
            leftColumn.style.width = new Length(50, LengthUnit.Percent);
            leftColumn.style.height = new Length(100, LengthUnit.Percent);
            leftColumn.style.justifyContent = Justify.Center;

            VisualElement rightColumn = new VisualElement();
            rightColumn.style.width = new Length(50, LengthUnit.Percent);
            rightColumn.style.height = new Length(100, LengthUnit.Percent);
            rightColumn.style.justifyContent = Justify.Center;
            rightColumn.style.alignItems = Align.Center;
            rightColumn.style.borderLeftWidth = 2;
            rightColumn.style.borderLeftColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

            var allSliders = settingsContainer.Query<Slider>().ToList();
            foreach (var slider in allSliders)
            {
                if (slider.name.Contains("Surface") || slider.name.Contains("Cave"))
                {
                    slider.style.display = DisplayStyle.None;
                    continue;
                }

                leftColumn.Add(slider);

                slider.style.marginTop = 20;
                slider.style.marginBottom = 20;
                slider.style.width = 380;
                slider.style.minWidth = 380;
                slider.style.height = 50;
                slider.style.alignSelf = Align.Center;
                slider.style.flexDirection = FlexDirection.Row;

                Label label = slider.Q<Label>();
                if (label != null)
                {
                    label.style.width = 160;
                    label.style.minWidth = 160;
                    label.style.fontSize = 24;
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;

                    label.style.paddingTop = 0;
                    label.style.paddingBottom = 0;
                    label.style.marginTop = 0;
                    label.style.marginBottom = 0;
                    label.style.paddingLeft = 15;
                }

                var dragContainer = slider.Q<VisualElement>("unity-drag-container");
                if (dragContainer != null)
                {
                    dragContainer.style.flexGrow = 1;
                    dragContainer.style.justifyContent = Justify.Center;
                    dragContainer.style.marginRight = 15;
                }

                var tracker = slider.Q<VisualElement>("unity-tracker");
                if (tracker != null)
                {
                    tracker.style.position = Position.Relative;
                    tracker.style.top = StyleKeyword.Auto;
                    tracker.style.marginTop = 0;
                    tracker.style.height = 16;
                    tracker.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                    tracker.style.borderTopWidth = 0;
                    tracker.style.borderBottomWidth = 0;
                    tracker.style.borderLeftWidth = 0;
                    tracker.style.borderRightWidth = 0;
                }

                var dragger = slider.Q<VisualElement>("unity-dragger");
                if (dragger != null)
                {
                    dragger.style.position = Position.Absolute;
                    dragger.style.top = new Length(50, LengthUnit.Percent);
                    dragger.style.marginTop = -20;
                    dragger.style.width = 40;
                    dragger.style.height = 40;
                    dragger.style.borderTopWidth = 0;
                    dragger.style.borderBottomWidth = 0;
                    dragger.style.borderLeftWidth = 0;
                    dragger.style.borderRightWidth = 0;
                    dragger.style.borderTopLeftRadius = 0;
                    dragger.style.borderTopRightRadius = 0;
                    dragger.style.borderBottomLeftRadius = 0;
                    dragger.style.borderBottomRightRadius = 0;
                }
            }

            if (controlsPanel != null)
            {
                controlsPanel.style.display = DisplayStyle.Flex;
                rightColumn.Add(controlsPanel);
            }

            VisualElement keybindsContainer = new VisualElement();

            keybindsContainer.style.width = new Length(100, LengthUnit.Percent);

            keybindsContainer.style.alignItems = Align.Center;

            keybindsContainer.style.position = Position.Absolute;
            keybindsContainer.style.top = 90;


            Label titleLabel = new Label("Controls");
            if (fontPixel != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(fontPixel);
            titleLabel.style.fontSize = 28;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 20;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            keybindsContainer.Add(titleLabel);


            string[] keybindsList = new string[]
            {
                "Move: A / D",
                "Jump & Double Jump: Space",
                "Wall Slide: Hold A / D on wall",
                "Attack: Right Click",
                "Mine: Left Click",
                "Consume: R",
                "Drop Item: Q",
                "Build: Middle Mouse - Mouse Wheel"
            };

            foreach (string bind in keybindsList)
            {
                Label bindLabel = new Label(bind);
                if (fontPixel != null) bindLabel.style.unityFontDefinition = new StyleFontDefinition(fontPixel);
                bindLabel.style.fontSize = 22;
                bindLabel.style.marginBottom = 15;
                bindLabel.style.whiteSpace = WhiteSpace.Normal;
                bindLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                keybindsContainer.Add(bindLabel);
            }

            if (rightColumn != null)
            {
                rightColumn.Add(keybindsContainer);
            }

            settingsContainer.Add(leftColumn);
            settingsContainer.Add(rightColumn);

            closeSettingsBtn = settingsContainer.Q<Button>("CloseSettingsBtn");
            if (closeSettingsBtn != null)
            {
                closeSettingsBtn.style.position = Position.Absolute;
                closeSettingsBtn.style.top = 20;
                closeSettingsBtn.style.right = 20;
                closeSettingsBtn.style.width = 100;
                closeSettingsBtn.style.height = 40;
                closeSettingsBtn.clicked += CloseSettings;
                closeSettingsBtn.BringToFront();
            }

            SetupAudioSlider(settingsContainer, "MasterSlider", "MasterVolume");
            SetupAudioSlider(settingsContainer, "MusicSlider", "MusicVolume");
            SetupAudioSlider(settingsContainer, "EntitiesSlider", "EntitiesVolume");
            SetupAudioSlider(settingsContainer, "AmbienceSlider", "AmbienceVolume");
        }

        private void SetupAudioSlider(VisualElement container, string sliderName, params string[] exposedParameterNames)
        {
            Slider volumeSlider = container.Q<Slider>(sliderName);
            if (volumeSlider != null)
            {
                volumeSlider.lowValue = 0.0001f;
                volumeSlider.highValue = 1f;

                if (mainAudioMixer != null && exposedParameterNames.Length > 0)
                {
                    float currentMixerVolume;
                    if (mainAudioMixer.GetFloat(exposedParameterNames[0], out currentMixerVolume))
                    {
                        volumeSlider.value = Mathf.Pow(10f, currentMixerVolume / 20f);
                    }
                }

                volumeSlider.RegisterValueChangedCallback(evt =>
                {
                    if (mainAudioMixer != null)
                    {
                        foreach (string param in exposedParameterNames)
                        {
                            mainAudioMixer.SetFloat(param, Mathf.Log10(evt.newValue) * 20f);
                        }
                    }
                });
            }
        }

        private void OpenSettings()
        {
            if (settingsContainer != null)
            {
                settingsContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingsContainer.style.display = DisplayStyle.None;
            }
        }

        public void UpdateHealth(int health, int maxHealth)
        {
            float percent = Mathf.Clamp01((float)health / (float)maxHealth);
            healthElm.style.width = Length.Percent(percent * 100f);
        }

        public void UpdateMod(float durationLeft, float duration)
        {
            float percent = Mathf.Clamp01(durationLeft / duration);
            modElm.style.width = Length.Percent(percent * 100f);
        }


        public string SerializeAll()
        {
            var entries = overlaysByBlockId.Values.Select(o => new OverlaySaveEntry
            {
                type = o.overlayType.ToString(),
                blockId = o.blockId,
                data = ((IInventory)o).ToJson()
            }).ToList();

            entries.Add(new OverlaySaveEntry
            {
                type = "Main",
                data = Items.Inventory.Singleton.ToJson()
            });

            return JsonUtility.ToJson(new SaveFile { overlays = entries.ToArray() });
        }

        public void DeserializeAll(string json)
        {
            Debug.Log($"De-Serializing aswell!");

            SaveFile save = JsonUtility.FromJson<SaveFile>(json);
            if (save?.overlays == null) return;

            foreach (OverlaySaveEntry entry in save.overlays)
            {
                if (entry.type == "Main")
                {
                    Items.Inventory.Singleton.FromJson(entry.data);
                    continue;
                }

                if (!Enum.TryParse(entry.type, out OverlayType type)) continue;

                Overlay overlay = CreateOverlay(entry.blockId, type);
                ((IInventory)overlay).FromJson(entry.data);
            }
        }

        public void SetItemName(string name)
        {
            itemNameElm.text = name;
        }

        private void Consume()
        {
            if (!playerInput.Player.Consume.WasPressedThisFrame()) return;
            ItemStack hand = Items.Inventory.Singleton.hand;
            if (hand == null || !hand.data.isConsumable || hand.data.equipmentType == Items.Overlays.EquipmentType.Mod) return;

            if (hand.data.name == "Slime Essence")
            {
                Health h = PlayerMovement.Singleton.GetComponent<Health>();
                int newHp = Mathf.Min(h.maxHealth, h.currentHealth + 25);
                h.SetHealth(newHp);
            }
            else if (hand.data.name == "Health Potion")
            {
                Health h = PlayerMovement.Singleton.GetComponent<Health>();
                int newHp = Mathf.Min(h.maxHealth, h.currentHealth + 80);
                h.SetHealth(newHp);
            }
            Items.Inventory.Singleton.RemoveFromHand();

        }

        private void Drop()
        {
            if (!playerInput.Player.Drop.WasPressedThisFrame()) return;
            ItemStack hand = Items.Inventory.Singleton.hand;
            if (hand != null)
            {
                ItemStack dropStack = new ItemStack(hand.data) { amount = 1, duration = hand.duration };
                Items.Inventory.Singleton.Drop(dropStack);
                Items.Inventory.Singleton.RemoveFromHand();
            }
        }

        #region JEI

        const short JEICols = 6;

        private VisualElement itemScroll;
        private VisualElement itemRecipe;

        private void SetupBook()
        {
            CloseBook();
            BackToList();

            ItemDatabase idb = Resources.Load<ItemDatabase>("ItemDatabase");
            RecipeDatabase rdb = Resources.Load<RecipeDatabase>("RecipeDatabase");
            CookingRecipeDatabase crdb = Resources.Load<CookingRecipeDatabase>("CookingRecipeDatabase");

            // itemSlots = new Items.Slot[idb.items.Count];

            int rows = idb.items.Count / JEICols;

            VisualElement itemHolder = recipeBookContainer.Q("items");

            for (short i = 0; i < idb.items.Count; i++)
            {
                UI.Components.Slot slot = new UI.Components.Slot(null, false, true)
                {
                    slotId = i,
                };
                ItemStack itemStack = new ItemStack(idb.items[i]);
                ItemData itemData = itemStack.data;

                UI.Components.Item item = new Item(null, false, false)
                {
                    item = itemData,
                    amount = 1,
                };

                slot.item = item;

                int col = i % JEICols;
                int row = i / JEICols;

                slot.AddToClassList("spaced-right");
                if (row != rows) slot.AddToClassList("spaced-bottom");
                itemHolder.Add(slot);
            }
        }

        public void BookItemClicked(string itemId)
        {
            itemScroll.style.display = DisplayStyle.None;
            itemRecipe.style.display = DisplayStyle.Flex;


        }

        public void BackToList()
        {
            itemScroll.style.display = DisplayStyle.Flex;
            itemRecipe.style.display = DisplayStyle.None;
        }

        public void ToggleBook()
        {
            if (isBookOpen) CloseBook();
            else OpenBook();
        }

        public void OpenBook()
        {
            isBookOpen = true;
            recipeBookContainer.style.display = DisplayStyle.Flex;
        }

        public void CloseBook()
        {
            isBookOpen = false;
            recipeBookContainer.style.display = DisplayStyle.None;
        }
        #endregion
        #endregion
    }
}
