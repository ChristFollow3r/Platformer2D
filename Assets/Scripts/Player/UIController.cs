using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using Data;
using Scriptable_Objects_Scripts;

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

        [Header("Elements")][SerializeField] private UIDocument overlay;
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

            #endregion
        }

        private void Start()
        {
            #region Start

            CloseOverlay();

            #endregion
        }

        private void Update()
        {
            #region Update
            CheckOverlay();
            MoveHand();
            foreach (Overlay overlay in overlaysByBlockId.Values)
            {
                overlay.Tick();
            }

            #endregion
        }

        #endregion

        #region Recipe Book Data

        [Header("Recipe Book")]
        [SerializeField]
        private RecipeDatabase craftingDatabase;

        [SerializeField] private CookingRecipeDatabase cookingDatabase;

        [SerializeField] private RuntimeAnimatorController defaultController;

        private int currentRecipeIndex = 0;

        private VisualElement recipeBookContainer;

        private Label leftTitleText;
        private VisualElement leftGridContainer;
        private Button prevPageBtn;

        private Label rightTitleText;
        private VisualElement rightGridContainer;
        private Button nextPageBtn;

        private Button closeRecipeBtn;

        #endregion

        #region Methods

        private void CreateUI()
        {
            #region CreateUI

            UI.Components.Inventory inventory = new UI.Components.Inventory();
            Hotbar overlayHotbar = new Hotbar { isMain = false };

            overlay.rootVisualElement.Q("inventory").Add(inventory);
            overlay.rootVisualElement.Q("holder").Add(overlayHotbar);

            nameShower = overlay.rootVisualElement.Q("name-holder");

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

            InitializeRecipeBook(menuRoot);
            InitializeControls(menuRoot);
            InitializeSettings(menuRoot);

            Button openMenuBtn = hud.rootVisualElement.Q<Button>("OpenMenu");
            if (openMenuBtn != null) openMenuBtn.clicked += ToggleMenu;

            Button backBtn = menuRoot.Q<Button>("Back");
            if (backBtn != null) backBtn.clicked += ToggleMenu;

            Button settingsBtn = menuRoot.Q<Button>("Settings");
            if (settingsBtn != null) settingsBtn.clicked += OpenSettings;

            Button recipesBtn = menuRoot.Q<Button>("Recipes");
            if (recipesBtn != null) recipesBtn.clicked += OpenRecipeBook;

            Button saveBtn = menuRoot.Q<Button>("Save");
            if (saveBtn != null) saveBtn.clicked += () => Debug.Log("Save");

            Button exitBtn = menuRoot.Q<Button>("Exit");
            if (exitBtn != null) exitBtn.clicked += () => Application.Quit();

            healthElm = hud.rootVisualElement.Q("health");
            modElm = hud.rootVisualElement.Q("mod");

            thankScreen.rootVisualElement.Q<Button>("continue").clicked += RemoveThankyouScreen;
            #endregion
        }

        public void ShowThankYouScreen()
        {
            Time.timeScale = 0f;
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

                CloseRecipeBook();
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

            if (playerInput.UI.CloseOverlay.WasPressedThisFrame() && isOverlayOpen) CloseOverlay();

            #endregion
        }

        public void OpenOverlay(ulong blockId)
        {
            #region OpenOverlay
            hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
            overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
            isOverlayOpen = true;

            if (!overlaysByBlockId.TryGetValue(blockId, out Overlay foundOverlay)) return;

            VisualElement overlayElement = CreateElement(foundOverlay);
            VisualElement left = overlay.rootVisualElement.Q("left");
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
            overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
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

        public void ShowName(string name, Vector2 pos)
        {
            nameShower.style.display = DisplayStyle.Flex;
            Label nameLb = nameShower.Q<Label>("name");
            nameLb.text = name;
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

            // Force the container to take up 100% of the right column's width
            keybindsContainer.style.width = new Length(100, LengthUnit.Percent);

            // This is the magic line that centers all the labels inside the container!
            keybindsContainer.style.alignItems = Align.Center;

            // Use Absolute positioning to force it to the top, ignoring standard spacing rules
            keybindsContainer.style.position = Position.Absolute;
            keybindsContainer.style.top = 90; // Decrease this number (e.g., 10 or 0) to move it even higher!


            // 2. Title Setup
            Label titleLabel = new Label("Controls");
            if (fontPixel != null) titleLabel.style.unityFontDefinition = new StyleFontDefinition(fontPixel);
            titleLabel.style.fontSize = 28;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 20;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter; // Center text inside the label
            keybindsContainer.Add(titleLabel);


            // 3. Keybinds Loop
            string[] keybindsList = new string[]
            {
                "Move: A / D",
                "Jump & Double Jump: Space",
                "Wall Slide: Hold A / D on wall",
                "Attack: Right Click",
                "Mine: Left Click",
                "Build: Middle Mouse - Mouse Wheel"
            };

            foreach (string bind in keybindsList)
            {
                Label bindLabel = new Label(bind);
                if (fontPixel != null) bindLabel.style.unityFontDefinition = new StyleFontDefinition(fontPixel);
                bindLabel.style.fontSize = 22;
                bindLabel.style.marginBottom = 15;
                bindLabel.style.whiteSpace = WhiteSpace.Normal;
                bindLabel.style.unityTextAlign = TextAnchor.MiddleCenter; // Center text inside the label
                keybindsContainer.Add(bindLabel);
            }

            // 4. Add to right column
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
            CloseRecipeBook();
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

        #endregion

        #region Recipe Book Logic

        private void InitializeRecipeBook(VisualElement menuRoot)
        {
            recipeBookContainer = menuRoot.Q<VisualElement>("RecipeBookContainer");
            if (recipeBookContainer == null) return;

            recipeBookContainer.style.position = Position.Absolute;
            recipeBookContainer.style.width = 800;
            recipeBookContainer.style.height = 600;
            recipeBookContainer.style.left = new Length(50, LengthUnit.Percent);
            recipeBookContainer.style.top = new Length(50, LengthUnit.Percent);
            recipeBookContainer.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent),
                new Length(-50, LengthUnit.Percent), 0));

            leftTitleText = recipeBookContainer.Q<Label>("LeftPageTitle");
            leftGridContainer = recipeBookContainer.Q<VisualElement>("LeftRecipeHolder");
            prevPageBtn = recipeBookContainer.Q<Button>("PrevPage");

            if (prevPageBtn != null)
            {
                prevPageBtn.style.position = Position.Absolute;
                prevPageBtn.style.bottom = 20;
                prevPageBtn.style.left = 20;
                prevPageBtn.clicked += () =>
                {
                    TurnPage(-1);
                    PlaySound(pageTurnSound);
                };
            }

            rightTitleText = recipeBookContainer.Q<Label>("RightPageTitle");
            rightGridContainer = recipeBookContainer.Q<VisualElement>("RightRecipeHolder");
            nextPageBtn = recipeBookContainer.Q<Button>("NextPage");

            if (nextPageBtn != null)
            {
                nextPageBtn.style.position = Position.Absolute;
                nextPageBtn.style.bottom = 20;
                nextPageBtn.style.right = 20;
                nextPageBtn.clicked += () =>
                {
                    TurnPage(1);
                    PlaySound(pageTurnSound);
                };
            }

            closeRecipeBtn = recipeBookContainer.Q<Button>("CloseRecipeBtn");

            if (closeRecipeBtn != null)
            {
                closeRecipeBtn.style.position = Position.Absolute;
                closeRecipeBtn.style.top = 20;
                closeRecipeBtn.style.left = 20;
                closeRecipeBtn.clicked += CloseRecipeBook;
            }

            recipeBookContainer.style.display = DisplayStyle.None;
        }

        private int GetTotalRecipes()
        {
            int craftingCount = craftingDatabase != null && craftingDatabase.recipes != null
                ? craftingDatabase.recipes.Count
                : 0;
            int cookingCount = cookingDatabase != null && cookingDatabase.recipes != null
                ? cookingDatabase.recipes.Count
                : 0;
            return craftingCount + cookingCount;
        }

        private void OpenRecipeBook()
        {
            if (GetTotalRecipes() == 0) return;

            CloseSettings();

            recipeBookContainer.style.display = DisplayStyle.Flex;
            currentRecipeIndex = 0;
            DisplayPages();
        }

        private void CloseRecipeBook()
        {
            if (recipeBookContainer != null)
            {
                recipeBookContainer.style.display = DisplayStyle.None;
            }
        }

        private void TurnPage(int direction)
        {
            int totalRecipes = GetTotalRecipes();
            int newIndex = currentRecipeIndex + (direction * 2);

            if (newIndex >= 0 && newIndex < totalRecipes)
            {
                currentRecipeIndex = newIndex;
                DisplayPages();
            }
        }

        private void DisplayPages()
        {
            int totalRecipes = GetTotalRecipes();

            PopulatePageData(currentRecipeIndex, leftTitleText, leftGridContainer);

            if (currentRecipeIndex + 1 < totalRecipes)
            {
                if (rightTitleText != null) rightTitleText.style.display = DisplayStyle.Flex;
                if (rightGridContainer != null) rightGridContainer.style.display = DisplayStyle.Flex;
                PopulatePageData(currentRecipeIndex + 1, rightTitleText, rightGridContainer);
            }
            else
            {
                if (rightTitleText != null) rightTitleText.text = "";
                if (rightGridContainer != null) rightGridContainer.Clear();
            }

            if (prevPageBtn != null)
                prevPageBtn.style.display = (currentRecipeIndex == 0) ? DisplayStyle.None : DisplayStyle.Flex;

            if (nextPageBtn != null)
                nextPageBtn.style.display =
                    (currentRecipeIndex + 2 >= totalRecipes) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void PopulatePageData(int index, Label titleLabel, VisualElement gridContainer)
        {
            if (titleLabel == null || gridContainer == null) return;

            gridContainer.Clear();

            int craftingCount = craftingDatabase != null && craftingDatabase.recipes != null
                ? craftingDatabase.recipes.Count
                : 0;

            ItemData resultItem = null;
            ItemData[] ingredientsArray = null;
            int displayColumns = 4;

            if (index < craftingCount)
            {
                var recipe = craftingDatabase.recipes[index];
                resultItem = recipe.result;
                ingredientsArray = recipe.ingredients;
                displayColumns = 4;
                if (titleLabel != null) titleLabel.text = "Crafting Table";
            }
            else
            {
                int cookingIndex = index - craftingCount;
                var recipe = cookingDatabase.recipes[cookingIndex];
                resultItem = recipe.result;
                ingredientsArray = recipe.ingredients;
                displayColumns = recipe.gridSize;
                if (titleLabel != null) titleLabel.text = "Furnace";
            }

            VisualElement gridWrapper = new VisualElement();
            float slotSize = 60f;

            gridWrapper.style.width = (displayColumns * slotSize) + 4;
            gridWrapper.style.flexDirection = FlexDirection.Row;
            gridWrapper.style.flexWrap = Wrap.Wrap;
            gridWrapper.style.justifyContent = Justify.Center;
            gridWrapper.style.marginBottom = 30;

            for (int i = 0; i < ingredientsArray.Length; i++)
            {
                VisualElement slot = new VisualElement();
                slot.style.width = slotSize;
                slot.style.height = slotSize;

                slot.style.borderTopWidth = 1;
                slot.style.borderBottomWidth = 1;
                slot.style.borderLeftWidth = 1;
                slot.style.borderRightWidth = 1;
                slot.style.borderTopColor = new StyleColor(Color.black);
                slot.style.borderBottomColor = new StyleColor(Color.black);
                slot.style.borderLeftColor = new StyleColor(Color.black);
                slot.style.borderRightColor = new StyleColor(Color.black);
                slot.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.2f));

                if (ingredientsArray[i] != null && ingredientsArray[i].sprite != null)
                {
                    Image icon = new Image();
                    icon.sprite = ingredientsArray[i].sprite;
                    icon.style.width = Length.Percent(100);
                    icon.style.height = Length.Percent(100);
                    slot.Add(icon);
                }

                gridWrapper.Add(slot);
            }

            gridContainer.Add(gridWrapper);

            VisualElement resultWrapper = new VisualElement();
            resultWrapper.style.alignItems = Align.Center;
            resultWrapper.style.flexDirection = FlexDirection.Column;

            VisualElement resultBox = new VisualElement();
            resultBox.style.width = 90;
            resultBox.style.height = 90;
            resultBox.style.borderTopWidth = 2;
            resultBox.style.borderBottomWidth = 2;
            resultBox.style.borderLeftWidth = 2;
            resultBox.style.borderRightWidth = 2;
            resultBox.style.borderTopColor = new StyleColor(Color.black);
            resultBox.style.borderBottomColor = new StyleColor(Color.black);
            resultBox.style.borderLeftColor = new StyleColor(Color.black);
            resultBox.style.borderRightColor = new StyleColor(Color.black);
            resultBox.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.4f));

            if (resultItem != null && resultItem.sprite != null)
            {
                Image resIcon = new Image();
                resIcon.sprite = resultItem.sprite;
                resIcon.style.width = Length.Percent(100);
                resIcon.style.height = Length.Percent(100);
                resultBox.Add(resIcon);
            }

            resultWrapper.Add(resultBox);

            if (resultItem != null)
            {
                Label resultLabel = new Label();
                resultLabel.text = resultItem.name;
                resultLabel.style.marginTop = 10;
                resultLabel.style.color = new StyleColor(Color.black);
                resultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                resultLabel.style.fontSize = 18;

                resultWrapper.Add(resultLabel);
            }

            gridContainer.Add(resultWrapper);
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
        #endregion
    }
}
