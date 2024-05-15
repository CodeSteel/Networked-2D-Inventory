using System;

namespace Game.InventorySystem
{
    [Serializable]
    public class InventoryItemSlot
    {
        public string ItemId;
        public int Amount;

        public bool IsEmpty => Amount == 0;
        
        public void Clear()
        {
            Amount = 0;
            ItemId = string.Empty;
        }

        public void Set(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
        
        public InventoryItemSlot()
        {
        }

        public InventoryItemSlot(InventoryItemSlot copy)
        {
            ItemId = copy.ItemId;
            Amount = copy.Amount;
        }
    }
}