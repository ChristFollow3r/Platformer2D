using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MainMenuUI : MonoBehaviour
{
    #region Singleton setup
    public static MainMenuUI Singleton;

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
    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioMixer mainAudioMixer;
    private AudioSource audioSource;

    [Header("UI Sprites")]
    [SerializeField] private Sprite sliderBarSprite;
    [SerializeField] private Sprite sliderKnobSprite;

    [Space(10)]
    [SerializeField] private UIDocument doc;
    [SerializeField] private WorldLoader loader;

    private VisualElement r => doc.rootVisualElement;
    private VisualElement menuPanel;
    private VisualElement newPanel;
    private VisualElement loadPanel;
    private VisualElement loadingPanel;
    private VisualElement settingsPanel;

    private List<Button> backBtns = new();

    #region New
    private TextField nameF;
    private TextField seedF;
    #endregion

    #region Load
    private ScrollView worldList;
    #endregion

    #region Loading
    public VisualElement loadFill;
    #endregion
    #endregion

    #region Unity
    private void Awake()
    {
        #region Awake
        Time.timeScale = 1f;

        SetupSingleton();
        audioSource = GetComponent<AudioSource>();
        GetElements();
        SubscribeEvents();
        SetupSettings();
        #endregion
    }
    #endregion

    #region Methods
    private void GetElements()
    {
        #region GetElements
        menuPanel = r.Q("MenuPanel");
        newPanel = r.Q("NewPanel");
        loadPanel = r.Q("LoadPanel");
        loadingPanel = r.Q("LoadingPanel");
        settingsPanel = r.Q("SettingsContainer");

        Button newBack = newPanel?.Q<Button>("Back");
        if (newBack != null) backBtns.Add(newBack);

        Button loadBack = loadPanel?.Q<Button>("Back");
        if (loadBack != null) backBtns.Add(loadBack);

        Button settingsBack = settingsPanel?.Q<Button>("Back");
        if (settingsBack != null) backBtns.Add(settingsBack);

        nameF = newPanel?.Q<TextField>("NameF");
        seedF = newPanel?.Q<TextField>("SeedF");

        worldList = loadPanel?.Q<ScrollView>("worldList");
        loadFill = loadingPanel?.Q("loadFill");
        #endregion
    }

    private void SubscribeEvents()
    {
        #region SubscribeEvents
        r.RegisterCallback<ClickEvent>(evt =>
        {
            VisualElement target = evt.target as VisualElement;

            while (target != null)
            {
                if (target is Button)
                {
                    PlayClickSound();
                    break;
                }
                target = target.parent;
            }
        });

        foreach (Button b in backBtns)
        {
            if (b != null)
            {
                b.clicked += () =>
                {
                    if (newPanel != null) newPanel.style.display = DisplayStyle.None;
                    if (loadPanel != null) loadPanel.style.display = DisplayStyle.None;
                    if (settingsPanel != null) settingsPanel.style.display = DisplayStyle.None;
                    if (menuPanel != null) menuPanel.style.display = DisplayStyle.Flex;
                };
            }
        }

        Button newBtn = menuPanel?.Q<Button>("New");
        if (newBtn != null)
        {
            newBtn.clicked += () =>
            {
                HandleNew();
                if (menuPanel != null) menuPanel.style.display = DisplayStyle.None;
                if (newPanel != null) newPanel.style.display = DisplayStyle.Flex;
            };
        }

        Button loadBtn = menuPanel?.Q<Button>("Load");
        if (loadBtn != null)
        {
            loadBtn.clicked += () =>
            {
                GetWorldList();
                if (menuPanel != null) menuPanel.style.display = DisplayStyle.None;
                if (loadPanel != null) loadPanel.style.display = DisplayStyle.Flex;
            };
        }

        Button settingsBtn = menuPanel?.Q<Button>("Settings");
        if (settingsBtn != null)
        {
            settingsBtn.clicked += () =>
            {
                if (menuPanel != null) menuPanel.style.display = DisplayStyle.None;
                if (settingsPanel != null) settingsPanel.style.display = DisplayStyle.Flex;
            };
        }

        Button exitBtn = menuPanel?.Q<Button>("Exit");
        if (exitBtn != null)
        {
            exitBtn.clicked += () =>
            {
                Application.Quit();
            };
        }

        Button createBtn = newPanel?.Q<Button>("Create");
        if (createBtn != null)
        {
            createBtn.clicked += HandleCreate;
        }
        #endregion
    }

    private void PlayClickSound()
    {
        #region PlayClickSound
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        #endregion
    }

    private void HandleNew()
    {
        #region HandleNew
        if (seedF == null) return;
        var rng = new System.Random();
        string randomSeed = (((long)rng.Next() << 32) | (long)(uint)rng.Next()).ToString();
        seedF.value = randomSeed;
        #endregion
    }

    private void HandleCreate()
    {
        #region HandleCreate
        if (seedF == null || nameF == null) return;
        if (string.IsNullOrEmpty(seedF.value) || string.IsNullOrEmpty(nameF.value)) return;
        loader.NewWorld(nameF.value, seedF.value);
        #endregion
    }

    private void GetWorldList()
    {
        #region GetWorldList
        if (worldList == null) return;
        string[] saves = Directory.GetDirectories(Application.persistentDataPath).Select(s => Path.GetFileName(s)).ToArray();

        worldList.Clear();
        foreach (string save in saves)
        {
            UI.Components.World world = new UI.Components.World()
            {
                worldName = save,
            };
            worldList.Add(world);
        }
        #endregion
    }

    public static void LoadWorld(string worldName)
    {
        Singleton.loader.LoadWorld(worldName);
    }

    public void SetLoading()
    {
        if (menuPanel != null) menuPanel.style.display = DisplayStyle.None;
        if (newPanel != null) newPanel.style.display = DisplayStyle.None;
        if (loadPanel != null) loadPanel.style.display = DisplayStyle.None;
        if (settingsPanel != null) settingsPanel.style.display = DisplayStyle.None;
        if (loadingPanel != null) loadingPanel.style.display = DisplayStyle.Flex;
    }

    private void SetupSettings()
    {
        if (settingsPanel == null) return;

        settingsPanel.style.minHeight = 350;
        settingsPanel.style.minWidth = 400;
        settingsPanel.style.paddingTop = 60;

        settingsPanel.style.flexDirection = FlexDirection.Column;
        settingsPanel.style.alignItems = Align.Center;
        settingsPanel.style.justifyContent = Justify.Center;

        var allSliders = settingsPanel.Query<Slider>().ToList();
        foreach (var s in allSliders)
        {
            if (s.name.Contains("Surface") || s.name.Contains("Cave"))
            {
                s.style.display = DisplayStyle.None;
            }
        }

        SetupAudioSlider(settingsPanel, "MasterSlider", "MasterVolume");
        SetupAudioSlider(settingsPanel, "MusicSlider", "MusicVolume");
        SetupAudioSlider(settingsPanel, "EntitiesSlider", "EntitiesVolume");
        SetupAudioSlider(settingsPanel, "AmbienceSlider", "AmbienceVolume");
    }

    private void SetupAudioSlider(VisualElement container, string sliderName, params string[] exposedParameterNames)
    {
        Slider volumeSlider = container.Q<Slider>(sliderName);
        if (volumeSlider != null)
        {
            StyleSlider(volumeSlider);

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

    private void StyleSlider(Slider slider)
    {
        // slider.style.marginTop = 10;
        // slider.style.marginBottom = 10;
        // slider.style.width = 280;
        // slider.style.minWidth = 280;
        // slider.style.height = 40;
        // slider.style.alignSelf = Align.Center;
        // slider.style.flexDirection = FlexDirection.Row;

        // Label label = slider.Q<Label>();
        // if (label != null)
        // {
        //     label.style.width = 120;
        //     label.style.minWidth = 120;
        //     label.style.fontSize = 24;
        //     label.style.unityTextAlign = TextAnchor.MiddleLeft;

        //     label.style.paddingTop = 0;
        //     label.style.paddingBottom = 0;
        //     label.style.marginTop = 0;
        //     label.style.marginBottom = 0;
        //     label.style.paddingLeft = 10;
        // }

        // var dragContainer = slider.Q<VisualElement>("unity-drag-container");
        // if (dragContainer != null)
        // {
        //     dragContainer.style.flexGrow = 1;
        //     dragContainer.style.justifyContent = Justify.Center;
        //     dragContainer.style.marginRight = 10;
        // }

        var tracker = slider.Q<VisualElement>("unity-tracker");
        if (tracker != null)
        {
            // tracker.style.position = Position.Relative;
            // tracker.style.top = StyleKeyword.Auto;
            // tracker.style.marginTop = 0;
            // tracker.style.height = 16;
            // tracker.style.borderTopWidth = 0;
            // tracker.style.borderBottomWidth = 0;
            // tracker.style.borderLeftWidth = 0;
            // tracker.style.borderRightWidth = 0;

            if (sliderBarSprite != null)
            {
                tracker.style.backgroundImage = new StyleBackground(sliderBarSprite);
                tracker.style.backgroundColor = new StyleColor(Color.clear);
            }
            else
            {
                tracker.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            }
        }

        var dragger = slider.Q<VisualElement>("unity-dragger");
        if (dragger != null)
        {
            dragger.style.position = Position.Absolute;
            dragger.style.top = new Length(50, LengthUnit.Percent);
            dragger.style.marginTop = -15;
            dragger.style.width = 30;
            dragger.style.height = 30;
            dragger.style.borderTopWidth = 0;
            dragger.style.borderBottomWidth = 0;
            dragger.style.borderLeftWidth = 0;
            dragger.style.borderRightWidth = 0;
            dragger.style.borderTopLeftRadius = 0;
            dragger.style.borderTopRightRadius = 0;
            dragger.style.borderBottomLeftRadius = 0;
            dragger.style.borderBottomRightRadius = 0;

            if (sliderKnobSprite != null)
            {
                dragger.style.backgroundImage = new StyleBackground(sliderKnobSprite);
                dragger.style.backgroundColor = new StyleColor(Color.clear);
            }
        }
    }
    #endregion
}
