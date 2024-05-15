using System.Collections.Generic;
using Game.EffectsSystem;
using Game.InventorySystem;
using SteelBox;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.ItemDatabaseSystem
{
    public class ItemDatabaseManager : BaseMonoBehaviour
    {
        [SerializeField]
        private SerializedDictionary<string, BaseItemSo> _inventoryItemSos = new SerializedDictionary<string, BaseItemSo>();
        [SerializeField]
        private SerializedDictionary<string, EffectsSo> _effectsItemSos = new SerializedDictionary<string, EffectsSo>();

        protected override void RegisterEvents()
        {
            ItemDatabaseHandlerData.OnGetItemById += OnGetItemById;
            ItemDatabaseHandlerData.OnGetEffectById += OnGetEffectById;
        }

        protected override void UnregisterEvents()
        {
            ItemDatabaseHandlerData.OnGetItemById -= OnGetItemById;
            ItemDatabaseHandlerData.OnGetEffectById -= OnGetEffectById;
        }
        
        private void Start()
        {
            BaseItemSo[] itemSos = Resources.LoadAll<BaseItemSo>("Items");
            foreach (BaseItemSo itemSo in itemSos)
            {
                _inventoryItemSos.Add(itemSo.Id, itemSo);
            }
            
            EffectsSo[] effectsSos = Resources.LoadAll<EffectsSo>("Items");
            foreach (EffectsSo effectSo in effectsSos)
            {
                _effectsItemSos.Add(effectSo.Id, effectSo);
            }
        }

        private void OnGetItemById(string id, ref BaseItemSo item)
        {
            _inventoryItemSos.TryGetValue(id, out item);
        }
        
        private void OnGetEffectById(string id, ref EffectsSo effectsSo)
        {
            _effectsItemSos.TryGetValue(id, out effectsSo);
        }
    }
}