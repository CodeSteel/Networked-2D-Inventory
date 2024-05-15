using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using Game.ActionSystem;
using Game.InventorySystem.Items;
using Game.ItemDatabaseSystem;
using Game.Logger;
using Game.PlayerSystems;
using Sirenix.OdinInspector;
using SteelBox;
using UnityEngine;

namespace Game.InventorySystem
{
    public struct InventorySyncData
    {
        public int SlotIndexRow;
        public int SlotIndexCol;
        public string ItemId;
        public int ItemAmount;
    }
    
    public class InventoryController : BaseNetworkBehaviour
    {
        private const float MaxPlayerInteractionDistance = 8f;
        
        [Header("Inventory Size")]
        [SerializeField] private int _baseColLimit;
        [SerializeField] private int _baseRowLimit;
        
        [SerializeField]
        private List<InventoryItemSlot> _itemsToStartWith = new List<InventoryItemSlot>();
        
        [ShowInInspector]
        private Inventory Inventory;

        // only send inventory updates to listeners
        private List<NetworkConnection> _listeners = new List<NetworkConnection>();
        private int _extraSlotsX;
        private int _extraSlotsY;
        
        protected override void RegisterEvents()
        {
            if (IsServerInitialized)
            {
                InventoryHandlerData.S_OnGiveItem += S_OnGiveItem;
                InventoryHandlerData.S_OnTakeItem += S_OnTakeItem;
                InventoryHandlerData.S_OnGetIsListening += S_OnGetIsListening;
                InventoryHandlerData.S_OnAddListener += S_OnAddListener;
                InventoryHandlerData.S_OnRemoveListener += S_OnRemoveListener;
                InventoryHandlerData.S_OnSetSlot += S_OnSetSlot;
                InventoryHandlerData.S_OnGetSlot += S_OnGetSlot;
                InventoryHandlerData.S_OnSetExtraSlotCount += S_OnSetExtraSlotCount;
            }

            if (IsClientInitialized)
            {
                InventoryHandlerData.C_OnMoveSlots += C_OnMoveSlots;
                InventoryHandlerData.C_OnMoveFromGearToInventory += C_OnMoveFromGearToInventory;
                InventoryHandlerData.C_OnMoveFromHotbarToInventory += C_OnMoveFromHotbarToInventory;
            }

            InventoryHandlerData.OnGetItemAmount += OnGetItemAmount;
            InventoryHandlerData.OnHasItem += OnHasItem;
            InventoryHandlerData.OnCanGiveItem += OnCanGiveItem;
            InventoryHandlerData.OnGetInventory += OnGetInventory;
        }

        protected override void UnregisterEvents()
        {
            if (IsServerInitialized)
            {
                InventoryHandlerData.S_OnGiveItem -= S_OnGiveItem;
                InventoryHandlerData.S_OnTakeItem -= S_OnTakeItem;
                InventoryHandlerData.S_OnGetIsListening -= S_OnGetIsListening;
                InventoryHandlerData.S_OnAddListener -= S_OnAddListener;
                InventoryHandlerData.S_OnRemoveListener -= S_OnRemoveListener;
                InventoryHandlerData.S_OnSetExtraSlotCount -= S_OnSetExtraSlotCount;
            }

            if (IsClientInitialized)
            {
                InventoryHandlerData.C_OnMoveSlots -= C_OnMoveSlots;
                InventoryHandlerData.C_OnMoveFromGearToInventory -= C_OnMoveFromGearToInventory;
                InventoryHandlerData.C_OnMoveFromHotbarToInventory -= C_OnMoveFromHotbarToInventory;
            }
            
            InventoryHandlerData.OnGetItemAmount -= OnGetItemAmount;
            InventoryHandlerData.OnHasItem -= OnHasItem;
            InventoryHandlerData.OnCanGiveItem -= OnCanGiveItem;
            InventoryHandlerData.OnGetInventory -= OnGetInventory;
            InventoryHandlerData.S_OnSetSlot -= S_OnSetSlot;
            InventoryHandlerData.S_OnGetSlot -= S_OnGetSlot;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Inventory = new Inventory(_baseColLimit, _baseRowLimit);
            StartCoroutine(S_DelayedSetupInventory());
        }

        [Server]
        private IEnumerator S_DelayedSetupInventory()
        {
            yield return new WaitForSeconds(0.1f);

            if (Owner != null)
            {
                S_OnAddListener(ObjectId, Owner);
            }
            
            // give start items
            foreach (InventoryItemSlot slot in _itemsToStartWith)
            {
                if (slot.Amount > 0)
                {
                    int leftover = 0;
                    InventoryHandlerData.S_GiveItem(ObjectId, slot.ItemId, slot.Amount, ref leftover);

                    if (Owner == null) continue;
                    
                    BaseItemSo itemSo = null;
                    ItemDatabaseHandlerData.GetItemById(slot.ItemId, ref itemSo);
                    if (itemSo == null) continue;
                    if (itemSo is ActionableItemSo actionItemSo)
                    {
                        ActionObjectHandlerData.S_EquipActionObject(OwnerId, actionItemSo);
                    }
                }
            }
        }

        [Server]
        private void S_OnGiveItem(int nobId, string itemId, int amount, ref int leftover)
        {
            if (nobId != ObjectId) return;
            
            BaseItemSo itemSo = null;
            ItemDatabaseHandlerData.GetItemById(itemId, ref itemSo);
            if (itemSo == null) return;
            leftover = Inventory.GiveItem(itemId, amount, out List<(int, int)> slotsModified);

            SLogger.Inventory.Log($"Giving {nobId} {itemId}x{amount} / {slotsModified.Count}");
            
            if (Owner != null)
                S_SyncSlotsToListeners(slotsModified);
        }

        [Server]
        private void S_SyncSlotsToListeners(List<(int, int)> slotsToSync)
        {
            if (_listeners.Count == 0) return;
            
            List<InventorySyncData> syncDatas = new List<InventorySyncData>();
            foreach ((int, int) slotIndex in slotsToSync)
            {
                syncDatas.Add(new InventorySyncData()
                {
                    SlotIndexCol = slotIndex.Item1,
                    SlotIndexRow = slotIndex.Item2,
                    ItemAmount = Inventory.Slots[slotIndex.Item1, slotIndex.Item2].Amount,
                    ItemId = Inventory.Slots[slotIndex.Item1, slotIndex.Item2].ItemId,
                });
            }

            S_VerifyListeners();
            foreach (NetworkConnection conn in _listeners)
            {
                C_TRpcSyncSlots(conn, syncDatas);
            }
        }
        
        [TargetRpc]
        private void C_TRpcSyncSlots(NetworkConnection _, List<InventorySyncData> syncDatas)
        {
            foreach (InventorySyncData syncData in syncDatas)
            {
                Inventory.Slots[syncData.SlotIndexCol, syncData.SlotIndexRow].Set(syncData.ItemId, syncData.ItemAmount);
            }
            InventoryHandlerData.C_InventoryChanged(ObjectId, Inventory.Slots);
        }

        [Server]
        private void S_SyncInventoryToListeners()
        {
            if (_listeners.Count == 0) return;
            
            List<InventorySyncData> syncDatas = new List<InventorySyncData>();
            for (int row = 0; row < Inventory.Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Inventory.Slots.GetLength(0); col++)
                {
                    syncDatas.Add(new InventorySyncData()
                    {
                        SlotIndexCol = col,
                        SlotIndexRow = row,
                        ItemAmount = Inventory.Slots[col,row].Amount,
                        ItemId = Inventory.Slots[col,row].ItemId,
                    });
                }
            }

            S_VerifyListeners();
            foreach (NetworkConnection conn in _listeners)
            {
                C_TRpcSyncInventory(conn, Inventory.Slots.GetLength(0), Inventory.Slots.GetLength(1), syncDatas);
            }
        }
        
        [TargetRpc]
        private void C_TRpcSyncInventory(NetworkConnection _, int colLimit, int rowLimit, List<InventorySyncData> syncDatas)
        {
            if (Inventory == null) Inventory = new Inventory(colLimit, rowLimit);
            foreach (InventorySyncData syncData in syncDatas)
            {
                Inventory.Slots[syncData.SlotIndexCol, syncData.SlotIndexRow].ItemId = syncData.ItemId;
                Inventory.Slots[syncData.SlotIndexCol, syncData.SlotIndexRow].Amount = syncData.ItemAmount;
            }
            InventoryHandlerData.C_InventoryChanged(ObjectId, Inventory.Slots);
        }
        
        [Server]
        private void S_VerifyListeners()
        {
            List<int> listenersToRemove = new List<int>();
            for (int i = 0; i < _listeners.Count; i++)
            {
                NetworkObject playerObj = _listeners[i].GetPlayerObject();
                if (playerObj == null) continue;
                if (Vector3.Distance(playerObj.transform.position, transform.position) > MaxPlayerInteractionDistance)
                {
                    listenersToRemove.Add(i);
                }
            }
            foreach (int i in listenersToRemove)
                _listeners.RemoveAt(i);
        }
        
        [Server]
        private void S_OnTakeItem(int nobId, string itemId, int amount)
        {
            if (nobId != ObjectId) return;
            
            BaseItemSo itemSo = null;
            ItemDatabaseHandlerData.GetItemById(itemId, ref itemSo);
            if (itemSo == null) return;
            Inventory.TakeItem(itemId, amount, out List<(int, int)> slotsModified);
            
            if (Owner != null) S_SyncSlotsToListeners(slotsModified);
        }

        [Server]
        private void S_OnGetIsListening(int controllerNobId, NetworkConnection conn, ref bool isListening)
        {
            if (controllerNobId != ObjectId) return;
            isListening = _listeners.Contains(conn);
        }
        
        [Server]
        private void S_OnAddListener(int controllerNobId, NetworkConnection conn)
        {
            if (controllerNobId != ObjectId) return;
            if (_listeners.Contains(conn)) return;
            _listeners.Add(conn);
            S_SyncInventoryToListeners();
        }
        
        [Server]
        private void S_OnRemoveListener(int controllerNobId, NetworkConnection conn)
        {
            if (controllerNobId != ObjectId) return;
            _listeners.Remove(conn);
        }

        [Server]
        private void S_OnSetExtraSlotCount(int controllerNobId, int extraSlotsX, int extraSlotsY)
        {
            if (controllerNobId != ObjectId) return;
            _extraSlotsX = extraSlotsX;
            _extraSlotsY = extraSlotsY;

            Inventory.Resize(_baseColLimit + _extraSlotsX, _baseRowLimit + _extraSlotsY);
            
            S_VerifyListeners();
            foreach (NetworkConnection conn in _listeners)
            {
                C_TRpcSyncResizeContainer(conn, extraSlotsX, extraSlotsY);
            }
        }
        
        [TargetRpc]
        private void C_TRpcSyncResizeContainer(NetworkConnection _, int extraSlotsX, int extraSlotsY)
        {
            _extraSlotsX = extraSlotsX;
            _extraSlotsY = extraSlotsY;
            Inventory.Resize(_baseColLimit + _extraSlotsX, _baseRowLimit + _extraSlotsY);
            InventoryHandlerData.C_InventoryChanged(ObjectId, Inventory.Slots);
        }

        private void C_OnMoveSlots(int fromContainerNobId, int toContainerNobId, (int,int) fromSlot, (int,int) toSlot)
        {
            if (toContainerNobId == fromContainerNobId && fromContainerNobId == ObjectId)
            {
                S_RpcMoveSameContainerSlots(fromSlot.Item1, fromSlot.Item2, toSlot.Item1, toSlot.Item2);
                return;
            }

            if (toContainerNobId == ObjectId)
            {
                S_RpcMoveDifferentContainersSlots(fromContainerNobId, fromSlot.Item1, fromSlot.Item2, toSlot.Item1, toSlot.Item2);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void S_RpcMoveSameContainerSlots(int fromCol, int fromRow, int toCol, int toRow, NetworkConnection conn = null)
        {
            if (!_listeners.Contains(conn)) return;
            if (!S_VerifyDistance(conn.GetPlayerObjectId())) return;
            
            InventoryItemSlot slotA = new InventoryItemSlot(Inventory.Slots[fromCol, fromRow]);
            InventoryItemSlot slotB = new InventoryItemSlot(Inventory.Slots[toCol, toRow]);

            Inventory.Slots[fromCol, fromRow] = slotB;
            Inventory.Slots[toCol, toRow] = slotA;
            
            S_SyncSlotsToListeners(new List<(int, int)>()
            {
                (fromCol, fromRow),
                (toCol, toRow)
            });
        }

        [ServerRpc(RequireOwnership = false)]
        private void S_RpcMoveDifferentContainersSlots(int fromContainerNobId, int fromCol, int fromRow, int toCol,
            int toRow, NetworkConnection conn = null)
        {
            if (!_listeners.Contains(conn)) return;
            if (!S_VerifyDistance(conn.GetPlayerObjectId())) return;

            InventoryItemSlot fromContainerSlot = default;
            InventoryHandlerData.S_GetSlot(fromContainerNobId, (fromCol, fromRow), ref fromContainerSlot);

            InventoryItemSlot myContainerSlot = Inventory.Slots[toCol, toRow];
            
            InventoryHandlerData.S_SetSlot(fromContainerNobId, (fromCol, fromRow), myContainerSlot);
            InventoryHandlerData.S_SetSlot(ObjectId, (toCol, toRow), fromContainerSlot);
        }
        
        [Client]
        private void C_OnMoveFromGearToInventory(int containerNobId, (int,int) targetSlot, EGearSlot fromGearSlot)
        {
            if (containerNobId != ObjectId) return;
            S_RpcMoveItemFromGearToInventory(targetSlot.Item1, targetSlot.Item2, fromGearSlot);
        }

        [ServerRpc(RequireOwnership = false)]
        private void S_RpcMoveItemFromGearToInventory(int targetSlotX, int targetSlotY, EGearSlot fromGearSlot, NetworkConnection conn = null)
        {
            if (!_listeners.Contains(conn)) return;
            int playerNobId = conn.GetPlayerObjectId();
            if (!S_VerifyDistance(playerNobId)) return;

            InventoryItemSlot toSlot = Inventory.Slots[targetSlotX, targetSlotY];

            // verify item is a GearItem with same EGearSlot
            if (!toSlot.IsEmpty)
            {
                BaseItemSo toSlotItemSo = default;
                ItemDatabaseHandlerData.GetItemById(toSlot.ItemId, ref toSlotItemSo);
                if (toSlotItemSo == default) return;

                if (toSlotItemSo is not GearItemSo gearItemSo) return;
                if (gearItemSo.GearSlot != fromGearSlot) return;
            }

            InventoryItemSlot fromSlot = default;
            GearHandlerData.S_GetGearSlot(playerNobId, fromGearSlot, ref fromSlot);
            if (fromSlot == default || fromSlot.IsEmpty) return;
            
            GearHandlerData.S_SetGearSlot(playerNobId, fromGearSlot, toSlot);
            InventoryHandlerData.S_SetSlot(ObjectId, (targetSlotX,targetSlotY), fromSlot);
        }

        [Client]
        private void C_OnMoveFromHotbarToInventory(int containerNobId, int fromSlot, (int, int) toSlot)
        {
            if (containerNobId != ObjectId) return;
            S_RpcMoveFromHotbarToInventory(fromSlot, toSlot.Item1, toSlot.Item2);
        }

        [ServerRpc(RequireOwnership = false)]
        private void S_RpcMoveFromHotbarToInventory(int fromSlot, int toSlotX, int toSlotY, NetworkConnection conn = null)
        {
            if (!_listeners.Contains(conn)) return;
            int playerNobId = conn.GetPlayerObjectId();
            if (!S_VerifyDistance(playerNobId)) return;
            
            InventoryItemSlot inventoryItemSlot = Inventory.Slots[toSlotX, toSlotY];

            InventoryItemSlot hotbarItemSlot = default;
            HotbarHandlerData.S_GetSlot(playerNobId, fromSlot, ref hotbarItemSlot);
            if (hotbarItemSlot == default || hotbarItemSlot.IsEmpty) return;
            
            HotbarHandlerData.S_SetSlot(playerNobId, fromSlot, inventoryItemSlot);
            InventoryHandlerData.S_SetSlot(ObjectId, (toSlotX, toSlotY), hotbarItemSlot);
        }

        [Server]
        private bool S_VerifyDistance(int playerNobId)
        {
            Vector3 position = Vector3.zero;
            PlayerHandlerData.S_GetPosition(playerNobId, ref position);

            if (Vector3.Distance(position, transform.position) > MaxPlayerInteractionDistance) return false; // dude's cheating
            
            return true;
        }

        [Server]
        private void S_OnSetSlot(int nobId, (int, int) slotIndex, InventoryItemSlot slot)
        {
            if (nobId != ObjectId) return;

            if (Inventory.Slots.GetLength(0) - 1 < slotIndex.Item1 || Inventory.Slots.GetLength(1) - 1 < slotIndex.Item2)
            {
                int leftOver = 0;
                InventoryHandlerData.S_GiveItem(ObjectId, slot.ItemId, slot.Amount, ref leftOver);
                return;
            }

            Inventory.Slots[slotIndex.Item1, slotIndex.Item2] = slot;
            S_SyncSlotsToListeners(new List<(int, int)>()
            {
                (slotIndex.Item1, slotIndex.Item2)
            });
        }
        
        [Server]
        private void S_OnGetSlot(int nobId, (int, int) slotIndex, ref InventoryItemSlot slot)
        {
            if (nobId != ObjectId) return;
            slot = Inventory.Slots[slotIndex.Item1, slotIndex.Item2];
        }
        
        #region Global Getters
        private void OnCanGiveItem(int nobId, string itemId, int amount, ref bool canGive)
        {
            if (nobId != ObjectId) return;
            BaseItemSo itemSo = null;
            ItemDatabaseHandlerData.GetItemById(itemId, ref itemSo);
            if (itemSo == null) return;
            canGive = Inventory.CanGiveItem(itemId, amount);
        }
        
        private void OnHasItem(int nobId, string itemId, int amount, ref bool hasItem)
        {
            if (nobId != ObjectId) return;
            hasItem = Inventory.HasItem(itemId, amount);
        }

        private void OnGetInventory(int nobId, ref InventoryItemSlot[,] inventorySlots)
        {
            if (nobId != ObjectId) return;
            inventorySlots = Inventory.Slots;
        }
        
        private void OnGetItemAmount(int nobId, string itemId, ref int amount)
        {
            if (nobId != ObjectId) return;
            amount = Inventory.GetAmount(itemId);
        }
        #endregion
    }
}