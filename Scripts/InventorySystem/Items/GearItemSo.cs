using UnityEngine;

namespace Game.InventorySystem.Items
{
    [CreateAssetMenu(menuName = "Game/GearSo")]
    public class GearItemSo : BaseItemSo
    {
        public EGearSlot GearSlot;
        
        public int AddedInventoryColumns;
        public int AddedInventoryRows;
    }
}