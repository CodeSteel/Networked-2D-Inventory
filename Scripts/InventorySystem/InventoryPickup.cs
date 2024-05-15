using Game.InteractionSystem;
using SteelBox;
using UnityEngine;

namespace Game.InventorySystem
{
    public class InventoryPickup : BaseNetworkBehaviour, IInteraction
    {
        [SerializeField] private InventoryItemSlot Item;
        
        protected override void RegisterEvents()
        {
        }

        protected override void UnregisterEvents()
        {
        }

        #region IInteractable
        public string InteractionText => $"Item: {Item.ItemId}, Amount: {Item.Amount}";

        public void Interact(int nobId, bool asServer)
        {
            if (!asServer) return;

            int leftoverAmount = 0;
            InventoryHandlerData.S_GiveItem(nobId, Item.ItemId, Item.Amount, ref leftoverAmount);
            Item.Amount = leftoverAmount;
            
            if (Item.Amount <= 0)
                Despawn(NetworkObject);
        }
        #endregion
    }
}