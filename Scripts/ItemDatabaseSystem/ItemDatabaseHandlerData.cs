using Game.EffectsSystem;
using Game.InventorySystem;
using SteelBox;

namespace Game.ItemDatabaseSystem
{
    public class ItemDatabaseHandlerData
    {
        public static event ActionValRef<string, BaseItemSo> OnGetItemById;
        public static void GetItemById(string id, ref BaseItemSo item) => OnGetItemById?.Invoke(id, ref item);
        
        public static event ActionValRef<string, EffectsSo> OnGetEffectById;
        public static void GetEffectById(string id, ref EffectsSo effectsSo) => OnGetEffectById?.Invoke(id, ref effectsSo);
    }
}