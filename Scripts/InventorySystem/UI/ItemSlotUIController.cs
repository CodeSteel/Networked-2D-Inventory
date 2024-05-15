using Game.InventorySystem.Items;
using Game.ItemDatabaseSystem;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.InventorySystem
{
    public class ItemSlotUIController : MonoBehaviour, IDropHandler
    {
        public bool IsHotbarSlot;
        [HideInInspector]
        public int HotbarSlot; // set by HotbarUIController
        
        public bool IsGearSlot;
        [ShowIf("IsGearSlot")]
        public EGearSlot GearSlot;
        
        public InventoryItemSlot ItemSlot;
        public DraggableItemSlotUI SlotContainer;
        
        private (int, int) _slotIndex;
        private int _containerNobId;

        private void Start()
        {
            SlotContainer = GetComponentInChildren<DraggableItemSlotUI>();
        }
        
        public void UpdateSlot(int containerNobId, (int, int) slotIndex, InventoryItemSlot slot)
        {
            _containerNobId = containerNobId;
            _slotIndex = slotIndex;
            UpdateSlot(slot);
        }

        public void UpdateSlot(InventoryItemSlot slot)
        {
            if (SlotContainer == null)
                SlotContainer = GetComponentInChildren<DraggableItemSlotUI>();
            
            TextMeshProUGUI itemAmountText = SlotContainer.ItemAmountText;
            Image itemIconImage = SlotContainer.ItemIconImage;
            
            ItemSlot = slot;
            if (slot == null || slot.IsEmpty)
            {
                itemAmountText.SetText(string.Empty);
                itemIconImage.sprite = null;
                itemIconImage.color = Color.clear;
                return;
            }

            BaseItemSo itemSo = null;
            ItemDatabaseHandlerData.GetItemById(ItemSlot.ItemId, ref itemSo);
            if (itemSo == null) return;
            
            itemAmountText.SetText(slot.Amount.ToString());
            itemIconImage.sprite = itemSo.Sprite;
            itemIconImage.color = Color.white;
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject droppedItemGameObject = eventData.pointerDrag;
            if (droppedItemGameObject == null) return;
            
            DraggableItemSlotUI droppedItem = droppedItemGameObject.GetComponent<DraggableItemSlotUI>();
            if (droppedItem == null) return;
            if (!droppedItem.TargetParent.TryGetComponent(out ItemSlotUIController droppedItemSlotController)) return;
            if (droppedItemSlotController.ItemSlot == null) return;
            if (droppedItemSlotController.ItemSlot.IsEmpty) return;
            
            DraggableItemSlotUI thisItem = GetComponentInChildren<DraggableItemSlotUI>();
            if (thisItem == null) return;
            
            BaseItemSo droppedItemSo = default;
            ItemDatabaseHandlerData.GetItemById(droppedItemSlotController.ItemSlot.ItemId, ref droppedItemSo);
            if (droppedItemSo == default) return;

            void SwitchParents()
            {
                (droppedItemSlotController.SlotContainer, SlotContainer) = (SlotContainer, droppedItemSlotController.SlotContainer);

                // update parents/positions
                thisItem.TargetParent = droppedItem.TargetParent;
                thisItem.transform.SetParent(thisItem.TargetParent);
                thisItem.transform.localPosition = Vector3.zero;
            
                droppedItem.TargetParent = transform;
                droppedItem.transform.SetParent(droppedItem.TargetParent);
                droppedItem.transform.localPosition = Vector3.zero;
            }
            
            // from inventory to gear slot
            if (IsGearSlot)
            {
                if (droppedItemSo is GearItemSo gearItemSo)
                {
                    if (GearSlot != gearItemSo.GearSlot) return;
                    
                    SwitchParents();
                    
                    // call move
                    if (droppedItemSlotController.IsHotbarSlot)
                        GearHandlerData.C_MoveFromHotbarToGear(droppedItemSlotController.HotbarSlot, gearItemSo.GearSlot);
                    else
                        GearHandlerData.C_MoveFromInventoryToGear(droppedItemSlotController._containerNobId, droppedItemSlotController._slotIndex, gearItemSo.GearSlot);
                }
                return;
            }
            
            // from gear slot to inventory slot
            if (droppedItemSlotController.IsGearSlot)
            {
                if (IsGearSlot) return;

                if (droppedItemSo is GearItemSo gearItemSo)
                {
                    // if this slot isn't empty, make sure we can move this slot to gear slot
                    if (ItemSlot != null && !ItemSlot.IsEmpty)
                    {
                        BaseItemSo thisSlotItemSo = default;
                        ItemDatabaseHandlerData.GetItemById(ItemSlot.ItemId, ref thisSlotItemSo);
                        if (thisSlotItemSo == default) return;
                        
                        if (thisSlotItemSo is not GearItemSo thisGearItemSo) return;
                        if (thisGearItemSo.GearSlot != gearItemSo.GearSlot) return;
                    }
                    
                    // from gear slot to this inventory slot
                    SwitchParents();

                    // call move
                    if (IsHotbarSlot)
                    {
                        HotbarHandlerData.C_MoveFromGearToHotbar(gearItemSo.GearSlot, HotbarSlot);
                    }
                    else
                    {
                        InventoryHandlerData.C_MoveFromGearToInventory(_containerNobId, _slotIndex, gearItemSo.GearSlot);
                    }
                }

                return;
            }
            
            SwitchParents();

            if (IsHotbarSlot)
            {
                if (droppedItemSlotController.IsHotbarSlot) // hotbar to hotbar
                {
                    HotbarHandlerData.C_MoveSlots(droppedItemSlotController.HotbarSlot, HotbarSlot);
                }
                else // container to hotbar
                {
                    HotbarHandlerData.C_MoveFromContainerToHotbar(droppedItemSlotController._containerNobId, droppedItemSlotController._slotIndex, HotbarSlot);
                }
            }
            else if (droppedItemSlotController.IsHotbarSlot) // hotbar to container
            {
                InventoryHandlerData.C_MoveFromHotbarToInventory(_containerNobId, droppedItemSlotController.HotbarSlot, _slotIndex);
            }
            else // container to container
            {
                InventoryHandlerData.C_MoveSlots(droppedItemSlotController._containerNobId, _containerNobId, droppedItemSlotController._slotIndex, _slotIndex);
            }
        }
    }
}