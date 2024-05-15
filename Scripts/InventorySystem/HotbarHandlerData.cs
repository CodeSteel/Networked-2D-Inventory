using System;
using SteelBox;

namespace Game.InventorySystem
{
    public static class HotbarHandlerData
    {
        #region Server Events
        public static event Action<int, int, InventoryItemSlot> S_OnSetSlot;
        public static void S_SetSlot(int nobId, int slotIndex, InventoryItemSlot slot) => S_OnSetSlot?.Invoke(nobId, slotIndex, slot);
        
        public static event ActionValValRef<int, int, InventoryItemSlot> S_OnGetSlot;
        public static void S_GetSlot(int nobId, int slotIndex, ref InventoryItemSlot slot) => S_OnGetSlot?.Invoke(nobId, slotIndex, ref slot);
        #endregion
        
        public static event Action<int, (int, int), int> C_OnMoveFromContainerToHotbar;
        public static void C_MoveFromContainerToHotbar(int fromContainer, (int,int) fromSlotIndex, int toSlotIndex) => C_OnMoveFromContainerToHotbar?.Invoke(fromContainer, fromSlotIndex, toSlotIndex);

        public static event Action<EGearSlot, int> C_OnMoveFromGearToHotbar;
        public static void C_MoveFromGearToHotbar(EGearSlot fromSlot, int toSlotIndex) => C_OnMoveFromGearToHotbar?.Invoke(fromSlot, toSlotIndex);
        
        public static event Action<int, int> C_OnMoveSlots;
        public static void C_MoveSlots(int fromSlot, int toSlotIndex) => C_OnMoveSlots?.Invoke(fromSlot, toSlotIndex);
        
        public static event Action<int, InventoryItemSlot> C_OnHotbarSlotChanged;
        public static void C_HotbarSlotChanged(int slotIndex, InventoryItemSlot slot) => C_OnHotbarSlotChanged?.Invoke(slotIndex, slot);
    }
}