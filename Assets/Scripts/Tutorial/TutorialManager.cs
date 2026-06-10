using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Player;
using Items;

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
        Block3_CraftUpgrade,
        Block3_UseUpgrade,
        Block4_Copper,
        Block5_Anchor,
        Block6_Bronze,
        Block7_Iron,
        Block8_Primordial,
        Block9_Destiny,
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

    private PlayerMovement playerMovement;
    private BreakAndPlace breakAndPlace;
    private bool isWaitingToAdvance = false;

    // Quest Tracking Flags
    private bool b1_wood, b1_resin, b1_table;
    private bool b2_fiber, b2_string, b2_rock, b2_resin, b2_rocks, b2_clay, b2_furnace;
    private bool b3_rocks, b3_smelt, b3_upgrade, b3_use;

    // NEW: Phase 4-9 Flags
    private bool b4_copperOre, b4_magic, b4_copperIngot, b4_copperUpgrade;
    private bool b5_stoneBlock, b5_magic, b5_slime, b5_anchor, b5_setSpawn;
    private bool b6_tinOre, b6_copperOre, b6_bronzeIngot, b6_compressed, b6_bronzeUpgrade;
    private bool b7_ironOre, b7_compressed, b7_greater, b7_ironUpgrade;
    private bool b8_slime, b8_greater, b8_emerald, b8_ruby, b8_topaz, b8_onyx, b8_primordialUpgrade;
    private bool b9_destinyStone, b9_interactDestiny;

    // Dictionaries to track cumulative item gathering
    private Dictionary<string, int> previousInventoryState = new Dictionary<string, int>();
    private Dictionary<string, int> cumulativeItemsGathered = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
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

        // Prime the inventory tracking so starting items aren't counted as newly gathered
        PrimeInventoryTracking();

        InitializeBasicGoals();
        ProcessBasicState();
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

    // ==========================================
    // CUMULATIVE INVENTORY TRACKING
    // ==========================================

    private void PrimeInventoryTracking()
    {
        if (Inventory.Singleton == null || Inventory.Singleton.slots == null) return;

        previousInventoryState.Clear();
        foreach (var slot in Inventory.Singleton.slots)
        {
            if (!slot.isEmpty && slot.item != null && slot.item.data != null)
            {
                string itemName = slot.item.data.name;
                if (!previousInventoryState.ContainsKey(itemName))
                    previousInventoryState[itemName] = 0;

                previousInventoryState[itemName] += slot.item.amount;
            }
        }
    }

    private void UpdateInventoryTracking()
    {
        if (Inventory.Singleton == null || Inventory.Singleton.slots == null) return;

        Dictionary<string, int> currentTotals = new Dictionary<string, int>();
        foreach (var slot in Inventory.Singleton.slots)
        {
            if (!slot.isEmpty && slot.item != null && slot.item.data != null)
            {
                string itemName = slot.item.data.name;
                if (!currentTotals.ContainsKey(itemName))
                    currentTotals[itemName] = 0;

                currentTotals[itemName] += slot.item.amount;
            }
        }

        foreach (var kvp in currentTotals)
        {
            string itemName = kvp.Key;
            int currentAmt = kvp.Value;
            int prevAmt = previousInventoryState.ContainsKey(itemName) ? previousInventoryState[itemName] : 0;

            if (currentAmt > prevAmt)
            {
                if (!cumulativeItemsGathered.ContainsKey(itemName))
                    cumulativeItemsGathered[itemName] = 0;

                cumulativeItemsGathered[itemName] += (currentAmt - prevAmt);
            }
        }

        previousInventoryState = currentTotals;
    }

    private int GetCumulativeItemAmount(string itemName)
    {
        return cumulativeItemsGathered.ContainsKey(itemName) ? cumulativeItemsGathered[itemName] : 0;
    }

    private int GetTotalItemAmount(string itemName)
    {
        if (Inventory.Singleton == null || Inventory.Singleton.slots == null) return 0;

        int total = 0;
        foreach (var slot in Inventory.Singleton.slots)
        {
            if (!slot.isEmpty && slot.item.data.name == itemName)
            {
                total += slot.item.amount;
            }
        }
        return total;
    }

    // ==========================================
    // QUEST LOGIC
    // ==========================================

    private void CheckInventoryQuests(int slotId, ItemStack item)
    {
        UpdateInventoryTracking();
        CheckInventoryQuests();
    }

    private void CheckInventoryQuests()
    {
        if (isWaitingToAdvance) return;

        if (currentPhase == TutorialPhase.Block1_Gather)
        {
            if (!b1_wood && GetCumulativeItemAmount("Wood") >= 12) { b1_wood = true; tutorialUI.CompleteGoal("b1_wood"); }
            if (!b1_resin && GetCumulativeItemAmount("Resin") >= 1) { b1_resin = true; tutorialUI.CompleteGoal("b1_resin"); }
            if (!b1_table && GetCumulativeItemAmount("Crafting Table") >= 1) { b1_table = true; tutorialUI.CompleteGoal("b1_table"); }

            if (b1_wood && b1_resin && b1_table) StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block2_Gather, 1f));
        }
        else if (currentPhase == TutorialPhase.Block2_Gather)
        {
            if (!b2_fiber && GetCumulativeItemAmount("Fiber") >= 18) { b2_fiber = true; tutorialUI.CompleteGoal("b2_fiber"); }
            if (!b2_string && GetCumulativeItemAmount("String") >= 6) { b2_string = true; tutorialUI.CompleteGoal("b2_string"); }
            if (!b2_rock && GetCumulativeItemAmount("Rock") >= 12) { b2_rock = true; tutorialUI.CompleteGoal("b2_rock"); }
            if (!b2_resin && GetCumulativeItemAmount("Resin") >= 6) { b2_resin = true; tutorialUI.CompleteGoal("b2_resin"); }
            if (!b2_rocks && GetCumulativeItemAmount("Rocks") >= 6) { b2_rocks = true; tutorialUI.CompleteGoal("b2_rocks"); }
            if (!b2_clay && GetCumulativeItemAmount("Clay") >= 6) { b2_clay = true; tutorialUI.CompleteGoal("b2_clay"); }
            if (!b2_furnace && GetCumulativeItemAmount("Furnace") >= 1) { b2_furnace = true; tutorialUI.CompleteGoal("b2_furnace"); }

            if (b2_fiber && b2_string && b2_rock && b2_resin && b2_rocks && b2_clay && b2_furnace)
            {
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block3_Smelt, smeltResinVideo, "Place the furnace and add fuel to smelt materials", 3f));
            }
        }
        else if (currentPhase == TutorialPhase.Block3_Smelt)
        {
            if (!b3_rocks && GetCumulativeItemAmount("Rocks") >= 3) { b3_rocks = true; tutorialUI.CompleteGoal("b3_rocks"); }
            if (!b3_smelt && GetCumulativeItemAmount("Melted Resin") >= 1) { b3_smelt = true; tutorialUI.CompleteGoal("b3_smelt"); }

            if (b3_rocks && b3_smelt)
            {
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block3_CraftUpgrade, stoneUpgradeVideo, "Craft upgrades to improve your stats", 3f));
            }
        }
        else if (currentPhase == TutorialPhase.Block3_CraftUpgrade)
        {
            if (!b3_upgrade && GetCumulativeItemAmount("Stone Upgrade") >= 1)
            {
                b3_upgrade = true;
                tutorialUI.CompleteGoal("b3_upgrade");
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block3_UseUpgrade, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block4_Copper)
        {
            if (!b4_copperOre && GetCumulativeItemAmount("Copper Ore") >= 16) { b4_copperOre = true; tutorialUI.CompleteGoal("b4_copperOre"); }
            if (!b4_magic && GetCumulativeItemAmount("Magic Essence") >= 8) { b4_magic = true; tutorialUI.CompleteGoal("b4_magic"); }
            if (!b4_copperIngot && GetCumulativeItemAmount("Copper Ingot") >= 4) { b4_copperIngot = true; tutorialUI.CompleteGoal("b4_copperIngot"); }
            if (!b4_copperUpgrade && GetCumulativeItemAmount("Copper Upgrade") >= 1) { b4_copperUpgrade = true; tutorialUI.CompleteGoal("b4_copperUpgrade"); }

            if (b4_copperOre && b4_magic && b4_copperIngot && b4_copperUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block5_Anchor, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block5_Anchor)
        {
            if (!b5_stoneBlock && GetCumulativeItemAmount("Stone Block") >= 8) { b5_stoneBlock = true; tutorialUI.CompleteGoal("b5_stoneBlock"); }
            if (!b5_magic && GetCumulativeItemAmount("Magic Essence") >= 4) { b5_magic = true; tutorialUI.CompleteGoal("b5_magic"); }
            if (!b5_slime && GetCumulativeItemAmount("Slime Essence") >= 4) { b5_slime = true; tutorialUI.CompleteGoal("b5_slime"); }
            if (!b5_anchor && GetCumulativeItemAmount("Respawn Anchor") >= 1) { b5_anchor = true; tutorialUI.CompleteGoal("b5_anchor"); }

            // b5_setSpawn is handled by the external trigger NotifySpawnPointSet()
            if (b5_stoneBlock && b5_magic && b5_slime && b5_anchor && b5_setSpawn)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block6_Bronze, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block6_Bronze)
        {
            if (!b6_tinOre && GetCumulativeItemAmount("Tin Ore") >= 32) { b6_tinOre = true; tutorialUI.CompleteGoal("b6_tinOre"); }
            if (!b6_copperOre && GetCumulativeItemAmount("Copper Ore") >= 32) { b6_copperOre = true; tutorialUI.CompleteGoal("b6_copperOre"); }
            if (!b6_bronzeIngot && GetCumulativeItemAmount("Bronze Ingot") >= 4) { b6_bronzeIngot = true; tutorialUI.CompleteGoal("b6_bronzeIngot"); }
            if (!b6_compressed && GetCumulativeItemAmount("Compressed Magic Essence") >= 8) { b6_compressed = true; tutorialUI.CompleteGoal("b6_compressed"); }
            if (!b6_bronzeUpgrade && GetCumulativeItemAmount("Bronze Upgrade") >= 1) { b6_bronzeUpgrade = true; tutorialUI.CompleteGoal("b6_bronzeUpgrade"); }

            if (b6_tinOre && b6_copperOre && b6_bronzeIngot && b6_compressed && b6_bronzeUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block7_Iron, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block7_Iron)
        {
            if (!b7_ironOre && GetCumulativeItemAmount("Iron Ore") >= 16) { b7_ironOre = true; tutorialUI.CompleteGoal("b7_ironOre"); }
            if (!b7_compressed && GetCumulativeItemAmount("Compressed Magic Essence") >= 8) { b7_compressed = true; tutorialUI.CompleteGoal("b7_compressed"); }
            if (!b7_greater && GetCumulativeItemAmount("Greater Magic Essence") >= 4) { b7_greater = true; tutorialUI.CompleteGoal("b7_greater"); }
            if (!b7_ironUpgrade && GetCumulativeItemAmount("Iron Upgrade") >= 1) { b7_ironUpgrade = true; tutorialUI.CompleteGoal("b7_ironUpgrade"); }

            if (b7_ironOre && b7_compressed && b7_greater && b7_ironUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block8_Primordial, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block8_Primordial)
        {
            if (!b8_slime && GetCumulativeItemAmount("Slime Essence") >= 4) { b8_slime = true; tutorialUI.CompleteGoal("b8_slime"); }
            if (!b8_greater && GetCumulativeItemAmount("Greater Magic Essence") >= 8) { b8_greater = true; tutorialUI.CompleteGoal("b8_greater"); }
            if (!b8_emerald && GetCumulativeItemAmount("Emerald") >= 1) { b8_emerald = true; tutorialUI.CompleteGoal("b8_emerald"); }
            if (!b8_ruby && GetCumulativeItemAmount("Ruby") >= 1) { b8_ruby = true; tutorialUI.CompleteGoal("b8_ruby"); }
            if (!b8_topaz && GetCumulativeItemAmount("Topaz") >= 1) { b8_topaz = true; tutorialUI.CompleteGoal("b8_topaz"); }
            if (!b8_onyx && GetCumulativeItemAmount("Onyx") >= 1) { b8_onyx = true; tutorialUI.CompleteGoal("b8_onyx"); }
            if (!b8_primordialUpgrade && GetCumulativeItemAmount("Primordial Upgrade") >= 1) { b8_primordialUpgrade = true; tutorialUI.CompleteGoal("b8_primordialUpgrade"); }

            if (b8_slime && b8_greater && b8_emerald && b8_ruby && b8_topaz && b8_onyx && b8_primordialUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block9_Destiny, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block9_Destiny)
        {
            if (!b9_destinyStone && GetCumulativeItemAmount("Destiny Stone") >= 1) { b9_destinyStone = true; tutorialUI.CompleteGoal("b9_destinyStone"); }

            // b9_interactDestiny is handled by external trigger NotifyDestinyStoneInteracted()
            if (b9_destinyStone && b9_interactDestiny)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Completed, 1f));
            }
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
                tutorialUI.AddGoal("b2_fiber", "Gather 18 Fiber");
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
                tutorialUI.AddGoal("b3_smelt", "Smelt 1 Resin");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block3_CraftUpgrade:
                tutorialUI.AddGoal("b3_upgrade", "Craft 1 Stone Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block3_UseUpgrade:
                tutorialUI.AddGoal("b3_use", "Use the Stone Upgrade");
                break;
            case TutorialPhase.Block4_Copper:
                tutorialUI.AddGoal("b4_copperOre", "Gather 16 Copper Ore");
                tutorialUI.AddGoal("b4_magic", "Gather 8 Magic Essence");
                tutorialUI.AddGoal("b4_copperIngot", "Make 4 Copper Ingot");
                tutorialUI.AddGoal("b4_copperUpgrade", "Craft 1 Copper Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block5_Anchor:
                tutorialUI.AddGoal("b5_stoneBlock", "Gather 8 Stone Block");
                tutorialUI.AddGoal("b5_magic", "Gather 4 Magic Essence");
                tutorialUI.AddGoal("b5_slime", "Gather 4 Slime Essence");
                tutorialUI.AddGoal("b5_anchor", "Craft 1 Respawn Anchor");
                tutorialUI.AddGoal("b5_setSpawn", "Set your spawn point");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block6_Bronze:
                tutorialUI.AddGoal("b6_tinOre", "Gather 32 Tin Ore");
                tutorialUI.AddGoal("b6_copperOre", "Gather 32 Copper Ore");
                tutorialUI.AddGoal("b6_bronzeIngot", "Make 4 Bronze Ingot");
                tutorialUI.AddGoal("b6_compressed", "Craft 8 Compressed Magic Essence");
                tutorialUI.AddGoal("b6_bronzeUpgrade", "Craft 1 Bronze Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block7_Iron:
                tutorialUI.AddGoal("b7_ironOre", "Gather 16 Iron Ore");
                tutorialUI.AddGoal("b7_compressed", "Craft 8 Compressed Magic Essence");
                tutorialUI.AddGoal("b7_greater", "Craft 4 Greater Magic Essence");
                tutorialUI.AddGoal("b7_ironUpgrade", "Craft 1 Iron Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block8_Primordial:
                tutorialUI.AddGoal("b8_slime", "Gather 4 Slime Essence");
                tutorialUI.AddGoal("b8_greater", "Craft 8 Greater Magic Essence");
                tutorialUI.AddGoal("b8_emerald", "Gather 1 Emerald");
                tutorialUI.AddGoal("b8_ruby", "Gather 1 Ruby");
                tutorialUI.AddGoal("b8_topaz", "Gather 1 Topaz");
                tutorialUI.AddGoal("b8_onyx", "Gather 1 Onyx");
                tutorialUI.AddGoal("b8_primordialUpgrade", "Craft 1 Primordial Upgrade");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Block9_Destiny:
                tutorialUI.AddGoal("b9_destinyStone", "Craft 1 Destiny Stone");
                tutorialUI.AddGoal("b9_interactDestiny", "Interact with the Destiny Stone");
                CheckInventoryQuests();
                break;
            case TutorialPhase.Completed:
                tutorialUI.HidePopup();
                tutorialUI.ClearGoals();
                break;
        }
    }

    // ==========================================
    // EXTERNAL TRIGGERS
    // ==========================================

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
        if (currentPhase == TutorialPhase.Block3_UseUpgrade && !b3_use)
        {
            b3_use = true;
            tutorialUI.CompleteGoal("b3_use");
            StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block4_Copper, 1f));
        }
    }

    public void NotifySpawnPointSet()
    {
        if (currentPhase == TutorialPhase.Block5_Anchor && !b5_setSpawn)
        {
            b5_setSpawn = true;
            tutorialUI.CompleteGoal("b5_setSpawn");
            CheckInventoryQuests(); // Triggers the check to advance if other items are gathered
        }
    }

    public void NotifyDestinyStoneInteracted()
    {
        if (currentPhase == TutorialPhase.Block9_Destiny && !b9_interactDestiny)
        {
            b9_interactDestiny = true;
            tutorialUI.CompleteGoal("b9_interactDestiny");
            CheckInventoryQuests(); // Triggers the check to complete the tutorial
        }
    }
}
