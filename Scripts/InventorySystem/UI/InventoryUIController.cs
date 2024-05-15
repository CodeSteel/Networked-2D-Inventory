using System.Collections.Generic;
using Game.InputSystem;
using Sirenix.OdinInspector;
using SteelBox;
using UnityEngine;

namespace Game.InventorySystem
{
    public class InventoryUIController : BaseMonoBehaviour
    {
        [SerializeField] private GameObject _inventoryRootObject;
        
        [SerializeField] private ItemSlotUIController _itemSlotPrefab;
        [SerializeField] private RectTransform _inventoryPanelContainer;
        [SerializeField] private RectTransform _inventoryParentPanel;

        [SerializeField] private float _heightPerInventoryRow = 68;
        [SerializeField] private float _baseInventoryParentHeight = 20;

        public bool IsActive => _inventoryRootObject.activeSelf;
        
        private ItemSlotUIController[,] _instancedItemSlots;
        private int _inventoryObjectId = -1;
        
        protected override void RegisterEvents()
        {
            InventoryHandlerData.C_OnInventoryChanged += C_OnInventoryChanged;
        }

        protected override void UnregisterEvents()
        {
            InventoryHandlerData.C_OnInventoryChanged -= C_OnInventoryChanged;
        }

        public void Toggle(bool toggle)
        {
            _inventoryRootObject.SetActive(toggle);
        }
        
        public void LinkController(int inventoryNobId)
        {
            _inventoryObjectId = inventoryNobId;
        }
        
        private void C_OnInventoryChanged(int nobId, InventoryItemSlot[,] items)
        {
            if (_inventoryObjectId != nobId) return;
            
            if (_instancedItemSlots == null || _instancedItemSlots.GetLength(0) != items.GetLength(0) ||
                _instancedItemSlots.GetLength(1) != items.GetLength(1))
            {
                SetupInventory((items.GetLength(0), items.GetLength(1)));
            }

            if (_instancedItemSlots == null) return;
            
            for (int row = 0; row < items.GetLength(1); row++)
            {
                for (int col = 0; col < items.GetLength(0); col++)
                {
                    _instancedItemSlots[col,row].UpdateSlot(_inventoryObjectId, (col,row), items[col,row]);
                }
            }
        }
        
        [Button]
        private void SetupInventory((int, int) size)
        {
            Dictionary<(int, int), InventoryItemSlot> destroyedSlots = new Dictionary<(int, int), InventoryItemSlot>();
            if (_instancedItemSlots != null)
            {
                for (int row = 0; row < _instancedItemSlots.GetLength(1); row++)
                {
                    for (int col = 0; col < _instancedItemSlots.GetLength(0); col++)
                    {
                        if (_instancedItemSlots[col, row] == null) continue;
                        destroyedSlots.Add((col,row), _instancedItemSlots[col,row].ItemSlot);
                        Destroy(_instancedItemSlots[col,row].gameObject);
                    }
                }
            }

            _instancedItemSlots = new ItemSlotUIController[size.Item1, size.Item2];
            for (int row = 0; row < size.Item2; row++)
            {
                for (int col = 0; col < size.Item1; col++)
                {
                    ItemSlotUIController itemSlotUIController = Instantiate(_itemSlotPrefab, _inventoryPanelContainer);
                    _instancedItemSlots[col,row] = itemSlotUIController;
                    itemSlotUIController.name = $"{itemSlotUIController.name} ({col},{row})";
                    if (destroyedSlots.TryGetValue((col, row), out InventoryItemSlot slot))
                        itemSlotUIController.UpdateSlot(_inventoryObjectId, (col,row), slot);
                    else
                        itemSlotUIController.UpdateSlot(_inventoryObjectId, (col,row), default);
                }
            }

            int rows = size.Item2;
            _inventoryParentPanel.sizeDelta = new Vector2(_inventoryParentPanel.sizeDelta.x, _baseInventoryParentHeight + (_heightPerInventoryRow * rows));
        }
    }
}