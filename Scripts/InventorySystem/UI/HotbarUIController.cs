using System.Collections.Generic;
using SteelBox;
using UnityEngine;

namespace Game.InventorySystem
{
    public class HotbarUIController : BaseMonoBehaviour
    {
        [SerializeField]
        private List<ItemSlotUIController> _hotbarUISlots = new List<ItemSlotUIController>();
        
        protected override void RegisterEvents()
        {
            HotbarHandlerData.C_OnHotbarSlotChanged += C_OnHotbarSlotChanged;
        }

        protected override void UnregisterEvents()
        {
            HotbarHandlerData.C_OnHotbarSlotChanged -= C_OnHotbarSlotChanged;
        }

        private void Start()
        {
            for (int i = 0; i < _hotbarUISlots.Count; i++)
            {
                _hotbarUISlots[i].HotbarSlot = i;
            }
        }

        private void C_OnHotbarSlotChanged(int slotIndex, InventoryItemSlot slot)
        {
            _hotbarUISlots[slotIndex].UpdateSlot(slot);
        }
    }
}