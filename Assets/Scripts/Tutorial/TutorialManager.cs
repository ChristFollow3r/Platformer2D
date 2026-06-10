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

    // NEW: Dictionaries to track cumulative item gathering
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

    private void HandleMove() { if (basicState == BasicControlState.Movement && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(1f)); }
    private void HandleAttack(Vector2 pos) { if (basicState == BasicControlState.Attacking && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(1f)); }
    private void HandleMine() { if (basicState == BasicControlState.Mining && !isWaitingToAdvance) StartCoroutine(AdvanceBasicTutorial(1f)); }
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
            StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block1_Gather, craftingTableVideo, "Open your inventory with I to craft basic items", 0f));
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
                tutorialUI.ShowPopup("Right Click to place a block");
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

    // Retained in case you need it for specific snapshot checks in the future
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

            if (b1_wood && b1_resin && b1_table)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block2_Gather, 1f));
            }
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
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block3_Smelt, smeltResinVideo, "Place the furnace and add fuel to smelt materials", 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block3_Smelt)
        {
            if (!b3_rocks && GetCumulativeItemAmount("Rocks") >= 3) { b3_rocks = true; tutorialUI.CompleteGoal("b3_rocks"); }
            if (!b3_smelt && GetCumulativeItemAmount("Melted Resin") >= 1) { b3_smelt = true; tutorialUI.CompleteGoal("b3_smelt"); }

            if (b3_rocks && b3_smelt)
            {
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block3_CraftUpgrade, stoneUpgradeVideo, "Craft upgrades to improve your stats", 1f));
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
            StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Completed, 1f));
        }
    }
}
