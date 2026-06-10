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
                StartCoroutine(AdvancePhaseWithVideo(TutorialPhase.Block4_CraftUpgrade, stoneUpgradeVideo, "Craft upgrades to improve your stats", 3f));
            }
        }
        else if (currentPhase == TutorialPhase.Block4_CraftUpgrade)
        {
            if (!b4_upgrade && GetCumulativeItemAmount("Stone Upgrade") >= 1)
            {
                b4_upgrade = true;
                tutorialUI.CompleteGoal("b4_upgrade");
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block5_UseUpgrade, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block7_Copper)
        {
            if (!b7_copperOre && GetCumulativeItemAmount("Copper Ore") >= 16) { b7_copperOre = true; tutorialUI.CompleteGoal("b7_copperOre"); }
            if (!b7_magic && GetCumulativeItemAmount("Magic Essence") >= 8) { b7_magic = true; tutorialUI.CompleteGoal("b7_magic"); }
            if (!b7_copperIngot && GetCumulativeItemAmount("Copper Ingot") >= 4) { b7_copperIngot = true; tutorialUI.CompleteGoal("b7_copperIngot"); }
            if (!b7_copperUpgrade && GetCumulativeItemAmount("Copper Upgrade") >= 1) { b7_copperUpgrade = true; tutorialUI.CompleteGoal("b7_copperUpgrade"); }

            if (b7_copperOre && b7_magic && b7_copperIngot && b7_copperUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block8_Anchor, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block8_Anchor)
        {
            if (!b8_stoneBlock && GetCumulativeItemAmount("Stone Block") >= 8) { b8_stoneBlock = true; tutorialUI.CompleteGoal("b8_stoneBlock"); }
            if (!b8_magic && GetCumulativeItemAmount("Magic Essence") >= 4) { b8_magic = true; tutorialUI.CompleteGoal("b8_magic"); }
            if (!b8_slime && GetCumulativeItemAmount("Slime Essence") >= 4) { b8_slime = true; tutorialUI.CompleteGoal("b8_slime"); }
            if (!b8_anchor && GetCumulativeItemAmount("Respawn Anchor") >= 1) { b8_anchor = true; tutorialUI.CompleteGoal("b8_anchor"); }

            if (b8_stoneBlock && b8_magic && b8_slime && b8_anchor && b8_setSpawn)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block9_Bronze, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block9_Bronze)
        {
            if (!b9_tinOre && GetCumulativeItemAmount("Tin Ore") >= 32) { b9_tinOre = true; tutorialUI.CompleteGoal("b9_tinOre"); }
            if (!b9_copperOre && GetCumulativeItemAmount("Copper Ore") >= 32) { b9_copperOre = true; tutorialUI.CompleteGoal("b9_copperOre"); }
            if (!b9_bronzeIngot && GetCumulativeItemAmount("Bronze Ingot") >= 4) { b9_bronzeIngot = true; tutorialUI.CompleteGoal("b9_bronzeIngot"); }
            if (!b9_compressed && GetCumulativeItemAmount("Compressed Magic Essence") >= 8) { b9_compressed = true; tutorialUI.CompleteGoal("b9_compressed"); }
            if (!b9_bronzeUpgrade && GetCumulativeItemAmount("Bronze Upgrade") >= 1) { b9_bronzeUpgrade = true; tutorialUI.CompleteGoal("b9_bronzeUpgrade"); }

            if (b9_tinOre && b9_copperOre && b9_bronzeIngot && b9_compressed && b9_bronzeUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block10_Iron, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block10_Iron)
        {
            if (!b10_ironOre && GetCumulativeItemAmount("Iron Ore") >= 16) { b10_ironOre = true; tutorialUI.CompleteGoal("b10_ironOre"); }
            if (!b10_compressed && GetCumulativeItemAmount("Compressed Magic Essence") >= 8) { b10_compressed = true; tutorialUI.CompleteGoal("b10_compressed"); }
            if (!b10_greater && GetCumulativeItemAmount("Greater Magic Essence") >= 4) { b10_greater = true; tutorialUI.CompleteGoal("b10_greater"); }
            if (!b10_ironUpgrade && GetCumulativeItemAmount("Iron Upgrade") >= 1) { b10_ironUpgrade = true; tutorialUI.CompleteGoal("b10_ironUpgrade"); }

            if (b10_ironOre && b10_compressed && b10_greater && b10_ironUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block11_Primordial, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block11_Primordial)
        {
            if (!b11_slime && GetCumulativeItemAmount("Slime Essence") >= 4) { b11_slime = true; tutorialUI.CompleteGoal("b11_slime"); }
            if (!b11_greater && GetCumulativeItemAmount("Greater Magic Essence") >= 8) { b11_greater = true; tutorialUI.CompleteGoal("b11_greater"); }
            if (!b11_emerald && GetCumulativeItemAmount("Emerald") >= 1) { b11_emerald = true; tutorialUI.CompleteGoal("b11_emerald"); }
            if (!b11_ruby && GetCumulativeItemAmount("Ruby") >= 1) { b11_ruby = true; tutorialUI.CompleteGoal("b11_ruby"); }
            if (!b11_topaz && GetCumulativeItemAmount("Topaz") >= 1) { b11_topaz = true; tutorialUI.CompleteGoal("b11_topaz"); }
            if (!b11_onyx && GetCumulativeItemAmount("Onyx") >= 1) { b11_onyx = true; tutorialUI.CompleteGoal("b11_onyx"); }
            if (!b11_primordialUpgrade && GetCumulativeItemAmount("Primordial Upgrade") >= 1) { b11_primordialUpgrade = true; tutorialUI.CompleteGoal("b11_primordialUpgrade"); }

            if (b11_slime && b11_greater && b11_emerald && b11_ruby && b11_topaz && b11_onyx && b11_primordialUpgrade)
            {
                StartCoroutine(AdvancePhaseWithDelay(TutorialPhase.Block12_Destiny, 1f));
            }
        }
        else if (currentPhase == TutorialPhase.Block12_Destiny)
        {
            if (!b12_destinyStone && GetCumulativeItemAmount("Destiny Stone") >= 1) { b12_destinyStone = true; tutorialUI.CompleteGoal("b12_destinyStone"); }

            if (b12_destinyStone && b12_interactDestiny)
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
}
