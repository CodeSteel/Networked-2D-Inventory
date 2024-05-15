using System;
using SteelBox;
using UnityEngine;

namespace Game.InventorySystem
{
    public class GearInventoryUIController : BaseMonoBehaviour
    {
        [SerializeField] private ItemSlotUIController _headSlotUIController;
        [SerializeField] private ItemSlotUIController _bodySlotUIController;
        [SerializeField] private ItemSlotUIController _legsSlotUIController;
        [SerializeField] private ItemSlotUIController _feetSlotUIController;
        [SerializeField] private ItemSlotUIController _backSlotUIController;
        
        protected override void RegisterEvents()
        {
            GearHandlerData.C_OnGearSlotChanged += C_OnGearSlotChanged;
        }

        protected override void UnregisterEvents()
        {
            GearHandlerData.C_OnGearSlotChanged -= C_OnGearSlotChanged;
        }

        private void C_OnGearSlotChanged(EGearSlot gearSlot, InventoryItemSlot itemSlot)
        {
            switch (gearSlot)
            {
                case EGearSlot.Head:
                    _headSlotUIController.UpdateSlot(itemSlot);
                    break;
                case EGearSlot.Body:
                    _bodySlotUIController.UpdateSlot(itemSlot);
                    break;
                case EGearSlot.Legs:
                    _legsSlotUIController.UpdateSlot(itemSlot);
                    break;
                case EGearSlot.Feet:
                    _feetSlotUIController.UpdateSlot(itemSlot);
                    break;
                case EGearSlot.Back:
                    _backSlotUIController.UpdateSlot(itemSlot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gearSlot), gearSlot, null);
            }
        }
    }
}