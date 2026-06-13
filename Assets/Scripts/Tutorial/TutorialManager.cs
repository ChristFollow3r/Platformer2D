using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Player;
using Items;
using Data;
using Enemies;

public class TutorialManager : MonoBehaviour
{



    public static TutorialManager Instance { get; private set; }

    public enum BasicControlState { Movement, Attacking, Mining, Placing, Done }
    public enum TutorialPhase
    {
        BasicControls,
        Block1_Gather,
        Block2_Gather,
        Block3_Smelt,
        Block4_CraftUpgrade,
        Block5_UseUpgrade,
        Block6_Consume,
        Block7_Copper,
        Block8_Anchor,
        Block9_Bronze,
        Block10_Iron,
        Block11_Primordial,
        Block12_Destiny,
        Completed
    }

    private BasicControlState basicState = BasicControlState.Movement;
    private TutorialPhase currentPhase = TutorialPhase.BasicControls;

    [Header("UI & Systems")]
    public TutorialUI tutorialUI;
    public VideoPlayer videoPlayer;

    [Header("Basic Control Videos")]
    public VideoClip movementClip;
    public VideoClip attackingClip;
    public VideoClip miningClip;
    public VideoClip placingClip;

    [Header("Advanced Quest Videos")]
    public VideoClip craftingTableVideo;
    public VideoClip smeltResinVideo;
    public VideoClip stoneUpgradeVideo;
    public VideoClip consumableClip;

    private PlayerMovement playerMovement;
    private BreakAndPlace breakAndPlace;
    private bool isWaitingToAdvance = false;

    private bool b1_wood, b1_resin, b1_table;
    private bool b2_fiber, b2_string, b2_rock, b2_resin, b2_rocks, b2_clay, b2_furnace;
    private bool b3_rocks, b3_smelt;
    private bool b4_upgrade;
    private bool b5_use;
    private bool b6_consume_slime;
    private bool b7_copperOre, b7_magic, b7_copperIngot, b7_copperUpgrade;
    private bool b8_stoneBlock, b8_magic, b8_slime, b8_anchor, b8_setSpawn;
    private bool b9_tinOre, b9_copperOre, b9_bronzeIngot, b9_compressed, b9_bronzeUpgrade;
    private bool b10_ironOre, b10_compressed, b10_greater, b10_ironUpgrade;
    private bool b11_slime, b11_greater, b11_emerald, b11_ruby, b11_topaz, b11_onyx, b11_primordialUpgrade;
    private bool b12_destinyStone, b12_interactDestiny;


    private bool isRestoring = false;
    // private Dictionary<string, int> previousInventoryState = new Dictionary<string, int>();
    // private Dictionary<string, int> cumulativeItemsGathered = new Dictionary<string, int>();
    // private Dictionary<string, int> cumulativeCrafted = new Dictionary<string, int>();



    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // StartCoroutine(WaitForPlayer());
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnMovePerformed -= HandleMove;
            playerMovement.OnAttackPerformed -= HandleAttack;
        }

        if (breakAndPlace != null)
        {
            breakAndPlace.OnBlockBroken -= HandleMine;
            breakAndPlace.OnPlacePerformed -= HandlePlace;
        }

        if (Inventory.Singleton != null)
        {
            Inventory.Singleton.OnSlotChanged -= CheckInventoryQuests;
        }
    }

    private IEnumerator WaitForPlayer()
    {
        while (PlayerMovement.Singleton == null || Inventory.Singleton == null)
        {
            yield return null;
        }
        EnemySpawner.Singleton.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(1);

        playerMovement = PlayerMovement.Singleton;
        breakAndPlace = playerMovement.GetComponent<BreakAndPlace>();

        if (playerMovement != null)
        {
            playerMovement.OnMovePerformed += HandleMove;
            playerMovement.OnAttackPerformed += HandleAttack;
        }

        if (breakAndPlace != null)
        {
            breakAndPlace.OnBlockBroken += HandleMine;
            breakAndPlace.OnPlacePerformed += HandlePlace;
        }

        Inventory.Singleton.OnSlotChanged += CheckInventoryQuests;


        if (isRestoring)
        {
            isRestoring = false;
            if (currentPhase == TutorialPhase.BasicControls)
            {
                InitializeBasicGoals();
                RestoreBasicControlTicks();
                ProcessBasicState();
            }
            else
            {
                ProcessCurrentPhase();
                yield return new WaitForSecondsRealtime(0.5f);
                RestoreCompletedGoals();
                CheckInventoryQuests();
            }
        }
        else
        {
            InitializeBasicGoals();
            ProcessBasicState();
        }
    }

    private void InitializeBasicGoals()
    {
        tutorialUI.ClearGoals();
        tutorialUI.AddGoal("MovementToggle", "Move and Jump");
        tutorialUI.AddGoal("AttackingToggle", "Attack");
        tutorialUI.AddGoal("MiningToggle", "Mine a Block");
        tutorialUI.AddGoal("PlacingBlock", "Place a Block");
    }

    private void HandleMove() { if (basicState == BasicControlState.Movement && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(2f)); }
    private void HandleAttack(Vector2 pos) { if (basicState == BasicControlState.Attacking && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(2f)); }
    private void HandleMine() { if (basicState == BasicControlState.Mining && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(2f)); }
    private void HandlePlace() { if (basicState == BasicControlState.Placing && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(1f)); }

    private IEnumerator AdvanceBasicTutorial(float delayTime)
    {
        isWaitingToAdvance = true;

        switch (basicState)
        {
            case BasicControlState.Movement: tutorialUI.CompleteGoal("MovementToggle"); break;
            case BasicControlState.Attacking: tutorialUI.CompleteGoal("AttackingToggle"); break;
            case BasicControlState.Mining: tutorialUI.CompleteGoal("MiningToggle"); break;
            case BasicControlState.Placing: tutorialUI.CompleteGoal("PlacingBlock"); break;
        }

        yield return new WaitForSeconds(delayTime);

        basicState++;

        if (basicState == BasicControlState.Done)
        {
            EnemySpawner.Singleton.gameObject.SetActive(true);
            StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block1_Gather, craftingTableVideo, "Open your inventory with I to craft basic items", 1f));
        }
        else
        {
            ProcessBasicState();
        }

        isWaitingToAdvance = false;
    }

    private void ProcessBasicState()
    {
        EnemySpawner.Singleton.gameObject.SetActive(false);
        switch (basicState)
        {
            case BasicControlState.Movement:
                videoPlayer.clip = movementClip; videoPlayer.Play();
                tutorialUI.ShowPopup("Use A and D to move\nSPACE to jump and double Jump");
                break;
            case BasicControlState.Attacking:
                videoPlayer.clip = attackingClip; videoPlayer.Play();
                tutorialUI.ShowPopup("Left Click to attack enemies or hit props");
                break;
            case BasicControlState.Mining:
                videoPlayer.clip = miningClip; videoPlayer.Play();
                tutorialUI.ShowPopup("Hold Shift + Left Click to mine blocks");
                break;
            case BasicControlState.Placing:
                videoPlayer.clip = placingClip; videoPlayer.Play();
                tutorialUI.ShowPopup("Right Click to place a block while holding it in your hand slot");
                break;
        }
    }

    // private void PrimeInventoryTracking()
    // {
    //     if (Inventory.Singleton == null || Inventory.Singleton.slots == null) return;

    //     previousInventoryState.Clear();
    //     foreach (var slot in Inventory.Singleton.slots)
    //     {
    //         if (!slot.isEmpty && slot.item != null && slot.item.data != null)
    //         {
    //             string itemName = slot.item.data.name;
    //             if (!previousInventoryState.ContainsKey(itemName))
    //                 previousInventoryState[itemName] = 0;

    //             previousInventoryState[itemName] += slot.item.amount;
    //         }
    //     }
    // }

    // private void UpdateInventoryTracking()
    // {
    //     if (Inventory.Singleton == null || Inventory.Singleton.slots == null) return;

    //     Dictionary<string, int> currentTotals = new Dictionary<string, int>();
    //     foreach (var slot in Inventory.Singleton.slots)
    //     {
    //         if (!slot.isEmpty && slot.item != null && slot.item.data != null)
    //         {
    //             string itemName = slot.item.data.name;
    //             if (!currentTotals.ContainsKey(itemName))
    //                 currentTotals[itemName] = 0;

    //             currentTotals[itemName] += slot.item.amount;
    //         }
    //     }

    //     foreach (var kvp in currentTotals)
    //     {
    //         string itemName = kvp.Key;
    //         int currentAmt = kvp.Value;
    //         int prevAmt = previousInventoryState.ContainsKey(itemName) ? previousInventoryState[itemName] : 0;

    //         if (currentAmt > prevAmt)
    //         {
    //             if (!cumulativeItemsGathered.ContainsKey(itemName))
    //                 cumulativeItemsGathered[itemName] = 0;

    //             cumulativeItemsGathered[itemName] += (currentAmt - prevAmt);
    //         }
    //     }

    //     previousInventoryState = currentTotals;
    // }

    // private int IsInInventory(string itemName)
    // {
    //     return cumulativeItemsGathered.ContainsKey(itemName) ? cumulativeItemsGathered[itemName] : 0;
    // }

    // private int GetTotalItemAmount(string itemName)
    // {
    //     if (Inventory.Singleton == null || Inventory.Singleton.slots == null) return 0;

    //     int total = 0;
    //     foreach (var slot in Inventory.Singleton.slots)
    //     {
    //         if (!slot.isEmpty && slot.item.data.name == itemName)
    //         {
    //             total += slot.item.amount;
    //         }
    //     }
    //     return total;
    // }

    private void CheckInventoryQuests(int slotId, ItemStack item)
    {
        // UpdateInventoryTracking();
        CheckInventoryQuests();
    }

    private bool IsInInventory(string itemName, int desiredAmount, ItemData extraItem = null, int extraAmount = 1)
    {
        int foundAmount = 0;

        if (Inventory.Singleton != null && Inventory.Singleton.slots != null)
        {
            foreach (var slot in Inventory.Singleton.slots)
            {
                if (slot.isEmpty || slot.item?.data == null) continue;
                if (slot.item.data.name != itemName) continue;
                foundAmount += slot.item.amount;
            }
        }

        if (extraItem != null && extraItem.name == itemName)
            foundAmount += extraAmount;

        return foundAmount >= desiredAmount;
    }

    private void CheckInventoryQuests()
    {
        if (isWaitingToAdvance) return;

        if (currentPhase == TutorialPhase.Block1_Gather)
        {
            if (!b1_wood && IsInInventory("Wood", 12)) { b1_wood = true; tutorialUI.CompleteGoal("b1_wood"); }
            if (!b1_resin && IsInInventory("Resin", 1)) { b1_resin = true; tutorialUI.CompleteGoal("b1_resin"); }
            if (b1_wood && b1_resin && b1_table) StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block2_Gather, 1f));
        }
        else if (currentPhase == TutorialPhase.Block2_Gather)
        {
            if (!b2_fiber && IsInInventory("Fiber", 9)) { b2_fiber = true; tutorialUI.CompleteGoal("b2_fiber"); }
            if (!b2_rock && IsInInventory("Rock", 12)) { b2_rock = true; tutorialUI.CompleteGoal("b2_rock"); }
            if (!b2_resin && IsInInventory("Resin", 6)) { b2_resin = true; tutorialUI.CompleteGoal("b2_resin"); }
            if (!b2_clay && IsInInventory("Clay", 6)) { b2_clay = true; tutorialUI.CompleteGoal("b2_clay"); }
            if (b2_fiber && b2_string && b2_rock && b2_resin && b2_rocks && b2_clay && b2_furnace)
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block3_Smelt, smeltResinVideo, "Place the furnace and add fuel to smelt materials", 3f));
        }
        else if (currentPhase == TutorialPhase.Block3_Smelt)
        {
            if (!b3_smelt && IsInInventory("Melted Resin", 1)) { b3_smelt = true; tutorialUI.CompleteGoal("b3_smelt"); }
            if (b3_rocks && b3_smelt)
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block4_CraftUpgrade, stoneUpgradeVideo, "Craft upgrades to improve your stats", 3f));
        }
        else if (currentPhase == TutorialPhase.Block7_Copper)
        {
            if (!b7_copperOre && IsInInventory("Copper Ore", 16)) { b7_copperOre = true; tutorialUI.CompleteGoal("b7_copperOre"); }
            if (!b7_magic && IsInInventory("Magic Essence", 8)) { b7_magic = true; tutorialUI.CompleteGoal("b7_magic"); }
            if (b7_copperOre && b7_magic && b7_copperIngot && b7_copperUpgrade)
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block8_Anchor, 1f));
        }
        else if (currentPhase == TutorialPhase.Block8_Anchor)
        {
            if (!b8_stoneBlock && IsInInventory("Stone Block", 8)) { b8_stoneBlock = true; tutorialUI.CompleteGoal("b8_stoneBlock"); }
            if (!b8_magic && IsInInventory("Magic Essence", 4)) { b8_magic = true; tutorialUI.CompleteGoal("b8_magic"); }
            if (!b8_slime && IsInInventory("Slime Essence", 4)) { b8_slime = true; tutorialUI.CompleteGoal("b8_slime"); }
            if (b8_stoneBlock && b8_magic && b8_slime && b8_anchor && b8_setSpawn)
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block9_Bronze, 1f));
        }
        else if (currentPhase == TutorialPhase.Block9_Bronze)
        {
            if (!b9_tinOre && IsInInventory("Tin Ore", 32)) { b9_tinOre = true; tutorialUI.CompleteGoal("b9_tinOre"); }
            if (!b9_copperOre && IsInInventory("Copper Ore", 32)) { b9_copperOre = true; tutorialUI.CompleteGoal("b9_copperOre"); }
            if (b9_tinOre && b9_copperOre && b9_bronzeIngot && b9_compressed && b9_bronzeUpgrade)
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block10_Iron, 1f));
        }
        else if (currentPhase == TutorialPhase.Block10_Iron)
        {
            if (!b10_ironOre && IsInInventory("Iron Ore", 16)) { b10_ironOre = true; tutorialUI.CompleteGoal("b10_ironOre"); }
            if (b10_ironOre && b10_compressed && b10_greater && b10_ironUpgrade)
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block11_Primordial, 1f));
        }
        else if (currentPhase == TutorialPhase.Block11_Primordial)
        {
            if (!b11_slime && IsInInventory("Slime Essence", 4)) { b11_slime = true; tutorialUI.CompleteGoal("b11_slime"); }
            if (!b11_emerald && IsInInventory("Emerald", 1)) { b11_emerald = true; tutorialUI.CompleteGoal("b11_emerald"); }
            if (!b11_ruby && IsInInventory("Ruby", 1)) { b11_ruby = true; tutorialUI.CompleteGoal("b11_ruby"); }
            if (!b11_topaz && IsInInventory("Topaz", 1)) { b11_topaz = true; tutorialUI.CompleteGoal("b11_topaz"); }
            if (!b11_onyx && IsInInventory("Onyx", 1)) { b11_onyx = true; tutorialUI.CompleteGoal("b11_onyx"); }
            if (b11_slime && b11_greater && b11_emerald && b11_ruby && b11_topaz && b11_onyx && b11_primordialUpgrade)
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block12_Destiny, 1f));
        }
        else if (currentPhase == TutorialPhase.Block12_Destiny)
        {
            if (b12_destinyStone && b12_interactDestiny)
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Completed, 1f));
        }
    }

    private void CheckCraftQuests(ItemData itemData, int amount = 1)
    {
        if (isWaitingToAdvance) return;

        switch (currentPhase)
        {
            case TutorialPhase.Block1_Gather:
                if (!b1_table && IsInInventory("Crafting Table", 1, itemData, amount)) { b1_table = true; tutorialUI.CompleteGoal("b1_table"); }
                if (b1_wood && b1_resin && b1_table) StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block2_Gather, 1f));
                break;

            case TutorialPhase.Block2_Gather:
                if (!b2_string && IsInInventory("String", 6, itemData, amount)) { b2_string = true; tutorialUI.CompleteGoal("b2_string"); }
                if (!b2_rocks && IsInInventory("Rocks", 6, itemData, amount)) { b2_rocks = true; tutorialUI.CompleteGoal("b2_rocks"); }
                if (!b2_furnace && IsInInventory("Furnace", 1, itemData, amount)) { b2_furnace = true; tutorialUI.CompleteGoal("b2_furnace"); }
                if (b2_fiber && b2_string && b2_rock && b2_resin && b2_rocks && b2_clay && b2_furnace)
                    StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block3_Smelt, smeltResinVideo, "Place the furnace and add fuel to smelt materials", 3f));
                break;

            case TutorialPhase.Block3_Smelt:
                if (!b3_rocks && IsInInventory("Rocks", 3, itemData, amount)) { b3_rocks = true; tutorialUI.CompleteGoal("b3_rocks"); }
                if (b3_rocks && b3_smelt)
                    StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block4_CraftUpgrade, stoneUpgradeVideo, "Craft upgrades to improve your stats", 3f));
                break;

            case TutorialPhase.Block4_CraftUpgrade:
                if (!b4_upgrade && IsInInventory("Stone Upgrade", 1, itemData, amount))
                {
                    b4_upgrade = true; tutorialUI.CompleteGoal("b4_upgrade");
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block5_UseUpgrade, 1f));
                }
                break;

            case TutorialPhase.Block7_Copper:
                if (!b7_copperIngot && IsInInventory("Copper Ingot", 4, itemData, amount)) { b7_copperIngot = true; tutorialUI.CompleteGoal("b7_copperIngot"); }
                if (!b7_copperUpgrade && IsInInventory("Copper Upgrade", 1, itemData, amount)) { b7_copperUpgrade = true; tutorialUI.CompleteGoal("b7_copperUpgrade"); }
                if (b7_copperOre && b7_magic && b7_copperIngot && b7_copperUpgrade)
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block8_Anchor, 1f));
                break;

            case TutorialPhase.Block8_Anchor:
                if (!b8_anchor && IsInInventory("Respawn Anchor", 1, itemData, amount)) { b8_anchor = true; tutorialUI.CompleteGoal("b8_anchor"); }
                if (b8_stoneBlock && b8_magic && b8_slime && b8_anchor && b8_setSpawn)
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block9_Bronze, 1f));
                break;

            case TutorialPhase.Block9_Bronze:
                if (!b9_bronzeIngot && IsInInventory("Bronze Ingot", 4, itemData, amount)) { b9_bronzeIngot = true; tutorialUI.CompleteGoal("b9_bronzeIngot"); }
                if (!b9_compressed && IsInInventory("Compressed Magic Essence", 8, itemData, amount)) { b9_compressed = true; tutorialUI.CompleteGoal("b9_compressed"); }
                if (!b9_bronzeUpgrade && IsInInventory("Bronze Upgrade", 1, itemData, amount)) { b9_bronzeUpgrade = true; tutorialUI.CompleteGoal("b9_bronzeUpgrade"); }
                if (b9_tinOre && b9_copperOre && b9_bronzeIngot && b9_compressed && b9_bronzeUpgrade)
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block10_Iron, 1f));
                break;

            case TutorialPhase.Block10_Iron:
                if (!b10_compressed && IsInInventory("Compressed Magic Essence", 8, itemData, amount)) { b10_compressed = true; tutorialUI.CompleteGoal("b10_compressed"); }
                if (!b10_greater && IsInInventory("Greater Magic Essence", 4, itemData, amount)) { b10_greater = true; tutorialUI.CompleteGoal("b10_greater"); }
                if (!b10_ironUpgrade && IsInInventory("Iron Upgrade", 1, itemData, amount)) { b10_ironUpgrade = true; tutorialUI.CompleteGoal("b10_ironUpgrade"); }
                if (b10_ironOre && b10_compressed && b10_greater && b10_ironUpgrade)
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block11_Primordial, 1f));
                break;

            case TutorialPhase.Block11_Primordial:
                if (!b11_greater && IsInInventory("Greater Magic Essence", 8, itemData, amount)) { b11_greater = true; tutorialUI.CompleteGoal("b11_greater"); }
                if (!b11_primordialUpgrade && IsInInventory("Primordial Upgrade", 1, itemData, amount)) { b11_primordialUpgrade = true; tutorialUI.CompleteGoal("b11_primordialUpgrade"); }
                if (b11_slime && b11_greater && b11_emerald && b11_ruby && b11_topaz && b11_onyx && b11_primordialUpgrade)
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block12_Destiny, 1f));
                break;

            case TutorialPhase.Block12_Destiny:
                if (!b12_destinyStone && IsInInventory("Destiny Stone", 1, itemData, amount)) { b12_destinyStone = true; tutorialUI.CompleteGoal("b12_destinyStone"); }
                if (b12_destinyStone && b12_interactDestiny)
                    StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Completed, 1f));
                break;
        }
    }

    private IEnumerator AdvancePhaseWithDelay(TutorialPhase nextPhase, float delay)
    {
        isWaitingToAdvance = true;
        yield return new WaitForSeconds(delay);
        currentPhase = nextPhase;
        ProcessCurrentPhase();
        isWaitingToAdvance = false;
    }

    private IEnumerator AdvancePhaseWithVideo(TutorialPhase nextPhase, VideoClip clip, string popupText, float delay)
    {
        isWaitingToAdvance = true;
        yield return new WaitForSeconds(delay);

        videoPlayer.clip = clip;
        videoPlayer.Play();
        tutorialUI.ShowPopup(popupText);

        currentPhase = nextPhase;
        ProcessCurrentPhase();
        isWaitingToAdvance = false;
    }

    private void ProcessCurrentPhase()
    {
        // PrimeInventoryTracking();
        // SeedCumulativeFromInventory();
        tutorialUI.ClearGoals();

        switch (currentPhase)
        {
            case TutorialPhase.Block1_Gather:
                tutorialUI.AddGoal("b1_wood", "Gather 12 Wood");
                tutorialUI.AddGoal("b1_resin", "Gather 1 Resin");
                tutorialUI.AddGoal("b1_table", "Make a Crafting Table");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block2_Gather:
                tutorialUI.AddGoal("b2_fiber", "Gather 9 Fiber");
                tutorialUI.AddGoal("b2_string", "Craft 6 String");
                tutorialUI.AddGoal("b2_rock", "Gather 12 Rock");
                tutorialUI.AddGoal("b2_resin", "Gather 6 Resin");
                tutorialUI.AddGoal("b2_rocks", "Craft 6 Rocks");
                tutorialUI.AddGoal("b2_clay", "Gather 6 Clay");
                tutorialUI.AddGoal("b2_furnace", "Craft 1 Furnace");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block3_Smelt:
                tutorialUI.AddGoal("b3_rocks", "Craft 3 Rocks");
                tutorialUI.AddGoal("b3_smelt", "Make 1 Smelted Resin");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block4_CraftUpgrade:
                tutorialUI.AddGoal("b4_upgrade", "Craft 1 Stone Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block5_UseUpgrade:
                tutorialUI.AddGoal("b5_use", "Use the Stone Upgrade");
                break;
            case TutorialPhase.Block6_Consume:
                tutorialUI.AddGoal("b6_consume_slime", "Consume 1 Slime Essence");
                break;
            case TutorialPhase.Block7_Copper:
                tutorialUI.AddGoal("b7_copperOre", "Gather 16 Copper Ore");
                tutorialUI.AddGoal("b7_magic", "Gather 8 Magic Essence");
                tutorialUI.AddGoal("b7_copperIngot", "Make 4 Copper Ingot");
                tutorialUI.AddGoal("b7_copperUpgrade", "Craft 1 Copper Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block8_Anchor:
                tutorialUI.AddGoal("b8_stoneBlock", "Gather 8 Stone Block");
                tutorialUI.AddGoal("b8_magic", "Gather 4 Magic Essence");
                tutorialUI.AddGoal("b8_slime", "Gather 4 Slime Essence");
                tutorialUI.AddGoal("b8_anchor", "Craft 1 Respawn Anchor");
                tutorialUI.AddGoal("b8_setSpawn", "Set your spawn point");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block9_Bronze:
                tutorialUI.AddGoal("b9_tinOre", "Gather 32 Tin Ore");
                tutorialUI.AddGoal("b9_copperOre", "Gather 32 Copper Ore");
                tutorialUI.AddGoal("b9_bronzeIngot", "Make 4 Bronze Ingot");
                tutorialUI.AddGoal("b9_compressed", "Craft 8 Compressed Magic Essence");
                tutorialUI.AddGoal("b9_bronzeUpgrade", "Craft 1 Bronze Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block10_Iron:
                tutorialUI.AddGoal("b10_ironOre", "Gather 16 Iron Ore");
                tutorialUI.AddGoal("b10_compressed", "Craft 8 Compressed Magic Essence");
                tutorialUI.AddGoal("b10_greater", "Craft 4 Greater Magic Essence");
                tutorialUI.AddGoal("b10_ironUpgrade", "Craft 1 Iron Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block11_Primordial:
                tutorialUI.AddGoal("b11_slime", "Gather 4 Slime Essence");
                tutorialUI.AddGoal("b11_greater", "Craft 8 Greater Magic Essence");
                tutorialUI.AddGoal("b11_emerald", "Gather 1 Emerald");
                tutorialUI.AddGoal("b11_ruby", "Gather 1 Ruby");
                tutorialUI.AddGoal("b11_topaz", "Gather 1 Topaz");
                tutorialUI.AddGoal("b11_onyx", "Gather 1 Onyx");
                tutorialUI.AddGoal("b11_primordialUpgrade", "Craft 1 Primordial Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block12_Destiny:
                tutorialUI.AddGoal("b12_destinyStone", "Craft 1 Destiny Stone");
                tutorialUI.AddGoal("b12_interactDestiny", "Interact with the Destiny Stone");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Completed:
                tutorialUI.HidePopup();
                tutorialUI.ClearGoals();
                break;
        }
    }

    public void NotifyResinSmelted()
    {
        if (currentPhase == TutorialPhase.Block3_Smelt && !b3_smelt)
        {
            b3_smelt = true;
            tutorialUI.CompleteGoal("b3_smelt");
            CheckInventoryQuests();
        }
    }

    public void NotifyStoneUpgradeUsed()
    {
        if (currentPhase == TutorialPhase.Block5_UseUpgrade && !b5_use)
        {
            b5_use = true;
            tutorialUI.CompleteGoal("b5_use");
            StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block6_Consume, consumableClip, "Pres R with a consumable in your hand slot to consume it", 1f));
        }
    }

    public void NotifySlimeEssenceConsumed()
    {
        if (currentPhase == TutorialPhase.Block6_Consume && !b6_consume_slime)
        {
            b6_consume_slime = true;
            tutorialUI.CompleteGoal("b6_consume_slime");
            StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block7_Copper, 1f));
        }
    }

    public void NotifySpawnPointSet()
    {
        if (currentPhase == TutorialPhase.Block8_Anchor && !b8_setSpawn)
        {
            b8_setSpawn = true;
            tutorialUI.CompleteGoal("b8_setSpawn");
            CheckInventoryQuests();
        }
    }

    public void NotifyDestinyStoneInteracted()
    {
        if (currentPhase == TutorialPhase.Block12_Destiny && !b12_interactDestiny)
        {
            b12_interactDestiny = true;
            tutorialUI.CompleteGoal("b12_interactDestiny");
            CheckInventoryQuests();
        }
    }

    // private void SeedCumulativeFromInventory()
    // {
    //     foreach (var kvp in previousInventoryState)
    //     {
    //         if (!cumulativeItemsGathered.ContainsKey(kvp.Key))
    //             cumulativeItemsGathered[kvp.Key] = 0;

    //         cumulativeItemsGathered[kvp.Key] = Mathf.Max(cumulativeItemsGathered[kvp.Key], kvp.Value);
    //     }
    // }

    // private int GetCumulativeCraftAmount(string itemName)
    // {
    //     return cumulativeCrafted.ContainsKey(itemName) ? cumulativeCrafted[itemName] : 0;
    // }


    public string Serialize()
    {
        TutorialData data = new()
        {
            basicState = (int)basicState,
            objectivesState = (int)currentPhase,

            b1_wood = b1_wood,
            b1_resin = b1_resin,
            b1_table = b1_table,
            b2_fiber = b2_fiber,
            b2_string = b2_string,
            b2_rock = b2_rock,
            b2_resin = b2_resin,
            b2_rocks = b2_rocks,
            b2_clay = b2_clay,
            b2_furnace = b2_furnace,
            b3_rocks = b3_rocks,
            b3_smelt = b3_smelt,
            b4_upgrade = b4_upgrade,
            b5_use = b5_use,
            b6_consume_slime = b6_consume_slime,
            b7_copperOre = b7_copperOre,
            b7_magic = b7_magic,
            b7_copperIngot = b7_copperIngot,
            b7_copperUpgrade = b7_copperUpgrade,
            b8_stoneBlock = b8_stoneBlock,
            b8_magic = b8_magic,
            b8_slime = b8_slime,
            b8_anchor = b8_anchor,
            b8_setSpawn = b8_setSpawn,
            b9_tinOre = b9_tinOre,
            b9_copperOre = b9_copperOre,
            b9_bronzeIngot = b9_bronzeIngot,
            b9_compressed = b9_compressed,
            b9_bronzeUpgrade = b9_bronzeUpgrade,
            b10_ironOre = b10_ironOre,
            b10_compressed = b10_compressed,
            b10_greater = b10_greater,
            b10_ironUpgrade = b10_ironUpgrade,
            b11_slime = b11_slime,
            b11_greater = b11_greater,
            b11_emerald = b11_emerald,
            b11_ruby = b11_ruby,
            b11_topaz = b11_topaz,
            b11_onyx = b11_onyx,
            b11_primordialUpgrade = b11_primordialUpgrade,
            b12_destinyStone = b12_destinyStone,
            b12_interactDestiny = b12_interactDestiny
        };

        return JsonUtility.ToJson(data);
    }

    public void Deserialize(string json)
    {
        TutorialData save = JsonUtility.FromJson<TutorialData>(json);

        basicState = (BasicControlState)save.basicState;
        currentPhase = (TutorialPhase)save.objectivesState;

        b1_wood = save.b1_wood; b1_resin = save.b1_resin; b1_table = save.b1_table;
        b2_fiber = save.b2_fiber; b2_string = save.b2_string; b2_rock = save.b2_rock;
        b2_resin = save.b2_resin; b2_rocks = save.b2_rocks; b2_clay = save.b2_clay; b2_furnace = save.b2_furnace;
        b3_rocks = save.b3_rocks; b3_smelt = save.b3_smelt;
        b4_upgrade = save.b4_upgrade;
        b5_use = save.b5_use;
        b6_consume_slime = save.b6_consume_slime;
        b7_copperOre = save.b7_copperOre; b7_magic = save.b7_magic; b7_copperIngot = save.b7_copperIngot; b7_copperUpgrade = save.b7_copperUpgrade;
        b8_stoneBlock = save.b8_stoneBlock; b8_magic = save.b8_magic; b8_slime = save.b8_slime; b8_anchor = save.b8_anchor; b8_setSpawn = save.b8_setSpawn;
        b9_tinOre = save.b9_tinOre; b9_copperOre = save.b9_copperOre; b9_bronzeIngot = save.b9_bronzeIngot; b9_compressed = save.b9_compressed; b9_bronzeUpgrade = save.b9_bronzeUpgrade;
        b10_ironOre = save.b10_ironOre; b10_compressed = save.b10_compressed; b10_greater = save.b10_greater; b10_ironUpgrade = save.b10_ironUpgrade;
        b11_slime = save.b11_slime; b11_greater = save.b11_greater; b11_emerald = save.b11_emerald; b11_ruby = save.b11_ruby;
        b11_topaz = save.b11_topaz; b11_onyx = save.b11_onyx; b11_primordialUpgrade = save.b11_primordialUpgrade;
        b12_destinyStone = save.b12_destinyStone; b12_interactDestiny = save.b12_interactDestiny;

        isRestoring = true;
        StartCoroutine(WaitForPlayer());
    }


    private void RestoreBasicControlTicks()
    {
        if (basicState > BasicControlState.Movement) tutorialUI.CompleteGoal("MovementToggle", true);
        if (basicState > BasicControlState.Attacking) tutorialUI.CompleteGoal("AttackingToggle", true);
        if (basicState > BasicControlState.Mining) tutorialUI.CompleteGoal("MiningToggle", true);
        if (basicState > BasicControlState.Placing) tutorialUI.CompleteGoal("PlacingBlock", true);
    }

    private void RestoreCompletedGoals()
    {
        switch (currentPhase)
        {
            case TutorialPhase.Block1_Gather:
                if (b1_wood) tutorialUI.CompleteGoal("b1_wood", true);
                if (b1_resin) tutorialUI.CompleteGoal("b1_resin", true);
                if (b1_table) tutorialUI.CompleteGoal("b1_table", true);
                break;
            case TutorialPhase.Block2_Gather:
                if (b2_fiber) tutorialUI.CompleteGoal("b2_fiber", true);
                if (b2_string) tutorialUI.CompleteGoal("b2_string", true);
                if (b2_rock) tutorialUI.CompleteGoal("b2_rock", true);
                if (b2_resin) tutorialUI.CompleteGoal("b2_resin", true);
                if (b2_rocks) tutorialUI.CompleteGoal("b2_rocks", true);
                if (b2_clay) tutorialUI.CompleteGoal("b2_clay", true);
                if (b2_furnace) tutorialUI.CompleteGoal("b2_furnace", true);
                break;
            case TutorialPhase.Block3_Smelt:
                if (b3_rocks) tutorialUI.CompleteGoal("b3_rocks", true);
                if (b3_smelt) tutorialUI.CompleteGoal("b3_smelt", true);
                break;
            case TutorialPhase.Block4_CraftUpgrade:
                if (b4_upgrade) tutorialUI.CompleteGoal("b4_upgrade", true);
                break;
            case TutorialPhase.Block5_UseUpgrade:
                if (b5_use) tutorialUI.CompleteGoal("b5_use", true);
                break;
            case TutorialPhase.Block6_Consume:
                if (b6_consume_slime) tutorialUI.CompleteGoal("b6_consume_slime", true);
                break;
            case TutorialPhase.Block7_Copper:
                if (b7_copperOre) tutorialUI.CompleteGoal("b7_copperOre", true);
                if (b7_magic) tutorialUI.CompleteGoal("b7_magic", true);
                if (b7_copperIngot) tutorialUI.CompleteGoal("b7_copperIngot", true);
                if (b7_copperUpgrade) tutorialUI.CompleteGoal("b7_copperUpgrade", true);
                break;
            case TutorialPhase.Block8_Anchor:
                if (b8_stoneBlock) tutorialUI.CompleteGoal("b8_stoneBlock", true);
                if (b8_magic) tutorialUI.CompleteGoal("b8_magic", true);
                if (b8_slime) tutorialUI.CompleteGoal("b8_slime", true);
                if (b8_anchor) tutorialUI.CompleteGoal("b8_anchor", true);
                if (b8_setSpawn) tutorialUI.CompleteGoal("b8_setSpawn", true);
                break;
            case TutorialPhase.Block9_Bronze:
                if (b9_tinOre) tutorialUI.CompleteGoal("b9_tinOre", true);
                if (b9_copperOre) tutorialUI.CompleteGoal("b9_copperOre", true);
                if (b9_bronzeIngot) tutorialUI.CompleteGoal("b9_bronzeIngot", true);
                if (b9_compressed) tutorialUI.CompleteGoal("b9_compressed", true);
                if (b9_bronzeUpgrade) tutorialUI.CompleteGoal("b9_bronzeUpgrade", true);
                break;
            case TutorialPhase.Block10_Iron:
                if (b10_ironOre) tutorialUI.CompleteGoal("b10_ironOre", true);
                if (b10_compressed) tutorialUI.CompleteGoal("b10_compressed", true);
                if (b10_greater) tutorialUI.CompleteGoal("b10_greater", true);
                if (b10_ironUpgrade) tutorialUI.CompleteGoal("b10_ironUpgrade", true);
                break;
            case TutorialPhase.Block11_Primordial:
                if (b11_slime) tutorialUI.CompleteGoal("b11_slime", true);
                if (b11_greater) tutorialUI.CompleteGoal("b11_greater", true);
                if (b11_emerald) tutorialUI.CompleteGoal("b11_emerald", true);
                if (b11_ruby) tutorialUI.CompleteGoal("b11_ruby", true);
                if (b11_topaz) tutorialUI.CompleteGoal("b11_topaz", true);
                if (b11_onyx) tutorialUI.CompleteGoal("b11_onyx", true);
                if (b11_primordialUpgrade) tutorialUI.CompleteGoal("b11_primordialUpgrade", true);
                break;
            case TutorialPhase.Block12_Destiny:
                if (b12_destinyStone) tutorialUI.CompleteGoal("b12_destinyStone", true);
                if (b12_interactDestiny) tutorialUI.CompleteGoal("b12_interactDestiny", true);
                break;
        }
    }

    public void OnCrafted(ItemData itemData, int amount = 1)
    {
        if (itemData == null || isWaitingToAdvance) return;

        // string itemName = itemData.name;
        // if (!cumulativeCrafted.ContainsKey(itemName))
        //     cumulativeCrafted[itemName] = 0;
        // cumulativeCrafted[itemName] += amount;

        CheckCraftQuests(itemData, amount);
    }

}
