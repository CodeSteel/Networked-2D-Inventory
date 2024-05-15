using System;
using FishNet.Connection;
using FishNet.Object;
using Game.InventorySystem.Items;
using Game.ItemDatabaseSystem;
using Game.Logger;
using Sirenix.OdinInspector;
using SteelBox;

namespace Game.InventorySystem
{
    public class GearInventoryController : BaseNetworkBehaviour
    {
        [ShowInInspector]
        private GearInventory Gear;
        
        protected override void RegisterEvents()
        {
            if (IsServerInitialized)
            {
                GearHandlerData.S_OnSetGearSlot += S_OnSetGearSlot;
                GearHandlerData.S_OnGetGearSlot += S_OnGetGearSlot;
            }

            if (IsClientInitialized && IsOwner)
            {
                GearHandlerData.C_OnMoveFromInventoryToGear += C_OnMoveFromInventoryToGear;
                GearHandlerData.C_OnMoveFromHotbarToGear += C_OnMoveFromHotbarToGear;
            }
        }

        protected override void UnregisterEvents()
        {
            if (IsServerInitialized)
            {
                GearHandlerData.S_OnSetGearSlot -= S_OnSetGearSlot;
                GearHandlerData.S_OnGetGearSlot -= S_OnGetGearSlot;
            }
            
            if (IsClientInitialized && IsOwner)
            {
                GearHandlerData.C_OnMoveFromInventoryToGear -= C_OnMoveFromInventoryToGear;
                GearHandlerData.C_OnMoveFromHotbarToGear -= C_OnMoveFromHotbarToGear;
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Gear = new GearInventory();
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
                Gear = new GearInventory();
        }
        
        [Server]
        private void S_OnSetGearSlot(int nobId, EGearSlot gearSlot, InventoryItemSlot itemSlot)
        {
            if (nobId != ObjectId) return;
            switch (gearSlot)
            {
                case EGearSlot.Head:
                    Gear.Head = itemSlot;
                    break;
                case EGearSlot.Body:
                    Gear.Body = itemSlot;
                    break;
                case EGearSlot.Legs:
                    Gear.Legs = itemSlot;
                    break;
                case EGearSlot.Feet:
                    Gear.Feet = itemSlot;
                    break;
                case EGearSlot.Back:
                    Gear.Back = itemSlot;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gearSlot), gearSlot, null);
            }

            GearHandlerData.S_GearSlotChanged(nobId, gearSlot, itemSlot);
            C_TRpcSetGearSlot(Owner, gearSlot, itemSlot);
            S_EquipGear(gearSlot);
        }

        [TargetRpc]
        private void C_TRpcSetGearSlot(NetworkConnection _, EGearSlot gearSlot, InventoryItemSlot itemSlot)
        {
            switch (gearSlot)
            {
                case EGearSlot.Head:
                    Gear.Head = itemSlot;
                    break;
                case EGearSlot.Body:
                    Gear.Body = itemSlot;
                    break;
                case EGearSlot.Legs:
                    Gear.Legs = itemSlot;
                    break;
                case EGearSlot.Feet:
                    Gear.Feet = itemSlot;
                    break;
                case EGearSlot.Back:
                    Gear.Back = itemSlot;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gearSlot), gearSlot, null);
            }
            GearHandlerData.C_GearSlotChanged(gearSlot, itemSlot);
        }

        [Server]
        private void S_OnGetGearSlot(int nobId, EGearSlot slot, ref InventoryItemSlot itemSlot)
        {
            if (nobId != ObjectId) return;
            itemSlot = Gear.GetSlot(slot);
        }

        [Client]
        private void C_OnMoveFromInventoryToGear(int fromContainerNobId, (int,int) fromSlot, EGearSlot toSlot)
        {
            S_RpcMoveFromInventoryToGear(fromContainerNobId, fromSlot.Item1, fromSlot.Item2, toSlot);
        }

        [ServerRpc]
        private void S_RpcMoveFromInventoryToGear(int fromContainerNobId, int fromSlotX, int fromSlotY, EGearSlot toSlot, NetworkConnection conn = null)
        {
            bool isListening = false;
            InventoryHandlerData.S_GetIsListening(fromContainerNobId, conn, ref isListening);
            if (!isListening) return;

            InventoryItemSlot inventoryItemSlot = default;
            InventoryHandlerData.S_GetSlot(fromContainerNobId, (fromSlotX, fromSlotY), ref inventoryItemSlot);
            if (inventoryItemSlot == default || inventoryItemSlot.IsEmpty) return;

            BaseItemSo itemSo = default;
            ItemDatabaseHandlerData.GetItemById(inventoryItemSlot.ItemId, ref itemSo);
            if (itemSo == default) return;

            if (itemSo is not GearItemSo gearItemSo) return;

            InventoryItemSlot currentItemInGearSlot = Gear.GetSlot(toSlot);
            if (currentItemInGearSlot == default) return;
            
            InventoryHandlerData.S_SetSlot(fromContainerNobId, (fromSlotX, fromSlotY), currentItemInGearSlot);
            GearHandlerData.S_SetGearSlot(ObjectId, toSlot, inventoryItemSlot);
        }

        [Client]
        private void C_OnMoveFromHotbarToGear(int fromSlot, EGearSlot toSlot)
        {
            S_RpcMoveFromHotbarToGear(fromSlot, toSlot);
        }

        [ServerRpc]
        private void S_RpcMoveFromHotbarToGear(int fromSlot, EGearSlot toSlot, NetworkConnection conn = null)
        {
            InventoryItemSlot hotbarSlot = default;
            HotbarHandlerData.S_GetSlot(ObjectId, fromSlot, ref hotbarSlot);
            if (hotbarSlot == default || hotbarSlot.IsEmpty) return;

            InventoryItemSlot gearSlot = Gear.GetSlot(toSlot);
            
            BaseItemSo hotbarItemSo = default;
            ItemDatabaseHandlerData.GetItemById(hotbarSlot.ItemId, ref hotbarItemSo);
            if (hotbarItemSo == default) return;

            if (hotbarItemSo is not GearItemSo gearItemSo) return;
            if (gearItemSo.GearSlot != toSlot) return;
            
            GearHandlerData.S_SetGearSlot(ObjectId, toSlot, hotbarSlot);
            HotbarHandlerData.S_SetSlot(ObjectId, fromSlot, gearSlot);
        }

        [Server]
        private void S_EquipGear(EGearSlot gearSlot)
        {
            InventoryItemSlot gearSlotItem = Gear.GetSlot(gearSlot);

            if (gearSlotItem == null || gearSlotItem.IsEmpty)
            {
                if (gearSlot == EGearSlot.Back)
                    InventoryHandlerData.S_SetExtraSlotCount(ObjectId, 0, 0); // TODO: separate into UnequipGear, ReplaceGear, EquipGear 
                return;
            }
            
            BaseItemSo baseGearItemSo = default;
            ItemDatabaseHandlerData.GetItemById(gearSlotItem.ItemId, ref baseGearItemSo);
            if (baseGearItemSo == default) return;
            if (baseGearItemSo is not GearItemSo gearItemSo) return;

            if (gearItemSo.AddedInventoryColumns > 0 || gearItemSo.AddedInventoryRows > 0)
            {
                InventoryHandlerData.S_SetExtraSlotCount(ObjectId, gearItemSo.AddedInventoryColumns, gearItemSo.AddedInventoryRows);
            }
        }
    }
}