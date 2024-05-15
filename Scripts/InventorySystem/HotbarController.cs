using FishNet.Connection;
using FishNet.Object;
using Sirenix.OdinInspector;
using SteelBox;
using UnityEngine;

namespace Game.InventorySystem
{
    public class HotbarController : BaseNetworkBehaviour
    {
        [SerializeField] private int _hotbarSlotCount;
        
        [ShowInInspector]
        private Inventory Hotbar;
        
        protected override void RegisterEvents()
        {
            if (IsServerInitialized)
            {
                HotbarHandlerData.S_OnSetSlot += S_OnSetSlot;
                HotbarHandlerData.S_OnGetSlot += S_OnGetSlot;
            }
            
            if (IsClientInitialized && IsOwner)
            {
                HotbarHandlerData.C_OnMoveFromContainerToHotbar += C_OnMoveFromContainerToHotbar;
                HotbarHandlerData.C_OnMoveFromGearToHotbar += C_OnMoveFromGearToHotbar;
                HotbarHandlerData.C_OnMoveSlots += C_OnMoveSlots;
            }
        }

        protected override void UnregisterEvents()
        {
            if (IsServerInitialized)
            {
                HotbarHandlerData.S_OnSetSlot -= S_OnSetSlot;
                HotbarHandlerData.S_OnGetSlot -= S_OnGetSlot;
            }

            if (IsClientInitialized && IsOwner)
            {
                HotbarHandlerData.C_OnMoveFromContainerToHotbar -= C_OnMoveFromContainerToHotbar;
                HotbarHandlerData.C_OnMoveFromGearToHotbar -= C_OnMoveFromGearToHotbar;
                HotbarHandlerData.C_OnMoveSlots -= C_OnMoveSlots;
            }
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            Hotbar = new Inventory(_hotbarSlotCount, 1);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
                Hotbar = new Inventory(_hotbarSlotCount, 1);
        }
        
        [Server]
        private void S_OnSetSlot(int nobId, int slotId, InventoryItemSlot itemSlot)
        {
            if (nobId != ObjectId) return;
            Hotbar.Slots[slotId, 0] = itemSlot;
            C_TRpcSetSlot(Owner, slotId, itemSlot);
        }

        [TargetRpc]
        private void C_TRpcSetSlot(NetworkConnection _, int slotId, InventoryItemSlot itemSlot)
        {
            Hotbar.Slots[slotId, 0] = itemSlot;
            HotbarHandlerData.C_HotbarSlotChanged(slotId, itemSlot);
        }
        
        [Server]
        private void S_OnGetSlot(int nobId, int slotId, ref InventoryItemSlot itemSlot)
        {
            if (nobId != ObjectId) return;
            itemSlot = Hotbar.Slots[slotId, 0];
        }

        [Client]
        private void C_OnMoveFromContainerToHotbar(int fromContainerNobId, (int, int) fromSlot, int toSlot)
        {
            S_RpcMoveFromContainerToHotbar(fromContainerNobId, fromSlot.Item1, fromSlot.Item2, toSlot);
        }

        [ServerRpc]
        private void S_RpcMoveFromContainerToHotbar(int containerNobId, int fromSlotX, int fromSlotY, int toSlot, NetworkConnection conn = null)
        {
            InventoryItemSlot hotbarSlot = new InventoryItemSlot(Hotbar.Slots[toSlot, 0]);
            
            InventoryItemSlot containerSlot = default;
            InventoryHandlerData.S_GetSlot(containerNobId, (fromSlotX,fromSlotY), ref containerSlot);
            
            InventoryHandlerData.S_SetSlot(containerNobId, (fromSlotX,fromSlotY), hotbarSlot);
            HotbarHandlerData.S_SetSlot(ObjectId, toSlot, containerSlot);
        }

        [Client]
        private void C_OnMoveFromGearToHotbar(EGearSlot fromSlot, int toSlot)
        {
            S_RpcMoveFromGearToHotbar(fromSlot, toSlot);
        }

        [ServerRpc]
        private void S_RpcMoveFromGearToHotbar(EGearSlot fromSlot, int toSlot, NetworkConnection conn = null)
        {
            InventoryItemSlot hotbarSlot = new InventoryItemSlot(Hotbar.Slots[toSlot, 0]);
            
            InventoryItemSlot gearSlot = default;
            GearHandlerData.S_GetGearSlot(ObjectId, fromSlot, ref gearSlot);
            if (gearSlot == default || gearSlot.IsEmpty) return;
            
            HotbarHandlerData.S_SetSlot(ObjectId, toSlot, gearSlot);
            GearHandlerData.S_SetGearSlot(ObjectId, fromSlot, hotbarSlot);
        }

        [Client]
        private void C_OnMoveSlots(int fromIndex, int toIndex)
        {
            S_RpcMoveSlots(fromIndex, toIndex);
        }

        [ServerRpc]
        private void S_RpcMoveSlots(int fromIndex, int toIndex, NetworkConnection conn = null)
        {
            InventoryItemSlot fromSlot = new InventoryItemSlot(Hotbar.Slots[fromIndex, 0]);
            InventoryItemSlot toSlot = new InventoryItemSlot(Hotbar.Slots[toIndex, 0]);

            HotbarHandlerData.S_SetSlot(ObjectId, fromIndex, toSlot);
            HotbarHandlerData.S_SetSlot(ObjectId, toIndex, fromSlot);
        }
    }
}