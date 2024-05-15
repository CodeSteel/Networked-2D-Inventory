using System;
using FishNet.Connection;
using SteelBox;

namespace Game.InventorySystem
{
    public static class InventoryHandlerData
    {
        #region Server Events
        public static event ActionValValValRef<int, string, int, int> S_OnGiveItem;
        public static void S_GiveItem(int nobId, string itemId, int amount, ref int leftover) => S_OnGiveItem?.Invoke(nobId, itemId, amount, ref leftover);

        public static event Action<int, string, int> S_OnTakeItem;
        public static void S_TakeItem(int nobId, string itemId, int amount) => S_OnTakeItem?.Invoke(nobId, itemId, amount);

        public static event ActionValValRef<int, NetworkConnection, bool> S_OnGetIsListening;
        public static void S_GetIsListening(int controllerNobId, NetworkConnection playerConn, ref bool isListening) => S_OnGetIsListening?.Invoke(controllerNobId, playerConn, ref isListening);
        
        public static event Action<int, (int,int), InventoryItemSlot> S_OnSetSlot;
        public static void S_SetSlot(int controllerNobId, (int,int) slotIndex, InventoryItemSlot slot) => S_OnSetSlot?.Invoke(controllerNobId, slotIndex, slot);
        
        public static event ActionValValRef<int, (int,int), InventoryItemSlot> S_OnGetSlot;
        public static void S_GetSlot(int controllerNobId, (int,int) slotIndex, ref InventoryItemSlot slot) => S_OnGetSlot?.Invoke(controllerNobId, slotIndex, ref slot);
        
        public static event Action<int, NetworkConnection> S_OnAddListener;
        public static void S_AddListener(int controllerNobId, NetworkConnection playerConn) => S_OnAddListener?.Invoke(controllerNobId, playerConn);
        
        public static event Action<int, NetworkConnection> S_OnRemoveListener;
        public static void S_RemoveListener(int controllerNobId, NetworkConnection playerConn) => S_OnRemoveListener?.Invoke(controllerNobId, playerConn);
        
        public static event Action<int, int, int> S_OnSetExtraSlotCount;
        public static void S_SetExtraSlotCount(int controllerNobId, int extraSlotX, int extraSlotY) => S_OnSetExtraSlotCount?.Invoke(controllerNobId, extraSlotX, extraSlotY);
        #endregion
        
        public static event ActionValValValRef<int, string, int, bool> OnCanGiveItem;
        public static void CanGiveItem(int nobId, string itemId, int amount, ref bool canGive) => OnCanGiveItem?.Invoke(nobId, itemId, amount, ref canGive);
        
        public static event ActionValValValRef<int, string, int, bool> OnHasItem;
        public static void HasItem(int nobId, string itemId, int amount, ref bool hasItem) => OnHasItem?.Invoke(nobId, itemId, amount, ref hasItem);
        
        public static event ActionValValRef<int, string, int> OnGetItemAmount;
        public static void GetItemAmount(int nobId, string itemId, ref int amount) => OnGetItemAmount?.Invoke(nobId, itemId, ref amount);
        
        public static event ActionValRef<int, InventoryItemSlot[,]> OnGetInventory;
        public static void GetInventory(int nobId, ref InventoryItemSlot[,] items) => OnGetInventory?.Invoke(nobId, ref items);

        #region Client Events
        public static event Action<int, InventoryItemSlot[,]> C_OnInventoryChanged;
        public static void C_InventoryChanged(int nobId, InventoryItemSlot[,] items) => C_OnInventoryChanged?.Invoke(nobId, items);
        
        public static event Action<int, int, (int,int), (int,int)> C_OnMoveSlots;
        public static void C_MoveSlots(int fromContainerNobId, int toContainerNobId, (int,int) fromSlot, (int,int) toSlot) => C_OnMoveSlots?.Invoke(fromContainerNobId, toContainerNobId, fromSlot, toSlot);

        public static event Action<int, (int, int), EGearSlot> C_OnMoveFromGearToInventory;
        public static void C_MoveFromGearToInventory(int containerNobId, (int,int) targetSlot, EGearSlot fromSlot) => C_OnMoveFromGearToInventory?.Invoke(containerNobId, targetSlot, fromSlot);
        
        public static event Action<int, int, (int,int)> C_OnMoveFromHotbarToInventory;
        public static void C_MoveFromHotbarToInventory(int containerNobId, int fromSlotIndex, (int,int) toSlotIndex) => C_OnMoveFromHotbarToInventory?.Invoke(containerNobId, fromSlotIndex, toSlotIndex);
        #endregion
    }
}