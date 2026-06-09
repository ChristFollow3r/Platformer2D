using System;

namespace Tutorial
{
    public static class TutorialEvents
    {
        public static event Action<string, int> OnItemUpdated;
        public static event Action OnCraftingTableInteracted;
        public static event Action OnResinSmelted;
        public static event Action OnStoneUpgradeUsed;

        public static void TriggerItemUpdated(string itemName, int amount) => OnItemUpdated?.Invoke(itemName, amount);
        public static void TriggerCraftingTableInteracted() => OnCraftingTableInteracted?.Invoke();
        public static void TriggerResinSmelted() => OnResinSmelted?.Invoke();
        public static void TriggerStoneUpgradeUsed() => OnStoneUpgradeUsed?.Invoke();
    }
}
