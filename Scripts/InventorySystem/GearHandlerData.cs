using System;
using SteelBox;

namespace Game.InventorySystem
{
    public static class GearHandlerData
    {
        public static void S_SetGearSlot(int nobId, EGearSlot gearSlot, InventoryItemSlot itemSlot) => S_OnSetGearSlot?.Invoke(nobId, gearSlot, itemSlot);
        public static event Action<int, EGearSlot, InventoryItemSlot> S_OnSetGearSlot;
        
        public static void S_GetGearSlot(int nobId, EGearSlot gearSlot, ref InventoryItemSlot itemSlot) => S_OnGetGearSlot?.Invoke(nobId, gearSlot, ref itemSlot);
        public static event ActionValValRef<int, EGearSlot, InventoryItemSlot> S_OnGetGearSlot;
        
        public static void S_GearSlotChanged(int nobId, EGearSlot gearSlot, InventoryItemSlot itemSlot) => S_OnGearSlotChanged?.Invoke(nobId, gearSlot, itemSlot);
        public static event Action<int, EGearSlot, InventoryItemSlot> S_OnGearSlotChanged;
        
        public static void C_GearSlotChanged(EGearSlot gearSlot, InventoryItemSlot itemSlot) => C_OnGearSlotChanged?.Invoke(gearSlot, itemSlot);
        public static event Action<EGearSlot, InventoryItemSlot> C_OnGearSlotChanged;
        
        public static event Action<int, (int,int), EGearSlot> C_OnMoveFromInventoryToGear;
        public static void C_MoveFromInventoryToGear(int fromContainerNobId, (int,int) fromSlot, EGearSlot toSlot) => C_OnMoveFromInventoryToGear?.Invoke(fromContainerNobId, fromSlot, toSlot);
        
        public static event Action<int, EGearSlot> C_OnMoveFromHotbarToGear;
        public static void C_MoveFromHotbarToGear(int fromSlot, EGearSlot toSlot) => C_OnMoveFromHotbarToGear?.Invoke(fromSlot, toSlot);
    }
}