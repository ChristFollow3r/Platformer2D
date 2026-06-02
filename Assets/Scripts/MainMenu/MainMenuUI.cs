using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
    private AudioSource audioSource;

    [Space(10)]
    [SerializeField] private UIDocument doc;
    [SerializeField] private WorldLoader loader;

    private VisualElement r => doc.rootVisualElement;
    private VisualElement menuPanel;
    private VisualElement newPanel;
    private VisualElement loadPanel;
    private VisualElement loadingPanel;

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
    /// <summary>Ran by unity on load</summary>
    private void Awake()
    {
        #region Awake
        SetupSingleton();

        // Grab the AudioSource component so we can play the sound
        audioSource = GetComponent<AudioSource>();

        GetElements();
        SubscribeEvents();
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

        backBtns.Add(newPanel.Q<Button>("Back"));
        backBtns.Add(loadPanel.Q<Button>("Back"));

        nameF = newPanel.Q<TextField>("NameF");
        seedF = newPanel.Q<TextField>("SeedF");

        worldList = loadPanel.Q<ScrollView>("worldList");
        loadFill = loadingPanel.Q("loadFill");

        #endregion
    }

    private void SubscribeEvents()
    {
        #region SubscribeEvents

        // Globally listen for clicks on the root visual element
        r.RegisterCallback<ClickEvent>(evt =>
        {
            VisualElement target = evt.target as VisualElement;

            // Traverse up to see if the clicked element is a Button (or inside one)
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

        backBtns.ForEach(b => b.clicked += () =>
            {
                newPanel.style.display = DisplayStyle.None;
                loadPanel.style.display = DisplayStyle.None;
                menuPanel.style.display = DisplayStyle.Flex;
            });

        menuPanel.Q<Button>("New").clicked += () =>
               {
                   HandleNew();
                   menuPanel.style.display = DisplayStyle.None;
                   newPanel.style.display = DisplayStyle.Flex;
               };
        menuPanel.Q<Button>("Load").clicked += () =>
        {
            GetWorldList();
            menuPanel.style.display = DisplayStyle.None;
            loadPanel.style.display = DisplayStyle.Flex;
        };
        menuPanel.Q<Button>("Settings").clicked += () =>
        {
            Debug.Log("Settings");
        };
        menuPanel.Q<Button>("Exit").clicked += () =>
        {
            Application.Quit();
        };

        newPanel.Q<Button>("Create").clicked += HandleCreate;
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
        var rng = new System.Random();
        string randomSeed = (((long)rng.Next() << 32) | (long)(uint)rng.Next()).ToString();
        seedF.value = randomSeed;
        #endregion
    }

    private void HandleCreate()
    {
        #region HandleCreate
        if (string.IsNullOrEmpty(seedF.value) || string.IsNullOrEmpty(nameF.value)) return;
        loader.NewWorld(nameF.value, seedF.value);
        #endregion
    }

    private void GetWorldList()
    {
        #region GetWorldList
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
        menuPanel.style.display = DisplayStyle.None;
        newPanel.style.display = DisplayStyle.None;
        loadPanel.style.display = DisplayStyle.None;
        loadingPanel.style.display = DisplayStyle.Flex;
    }
    #endregion
}
