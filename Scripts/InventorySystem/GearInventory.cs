using System;

namespace Game.InventorySystem
{
    public enum EGearSlot
    {
        Head,
        Body,
        Legs,
        Feet,
        Back
    }
    
    [Serializable]
    public class GearInventory
    {
        public InventoryItemSlot Head;
        public InventoryItemSlot Body;
        public InventoryItemSlot Legs;
        public InventoryItemSlot Feet;
        public InventoryItemSlot Back;

        public GearInventory()
        {
            Head = new InventoryItemSlot();
            Body = new InventoryItemSlot();
            Legs = new InventoryItemSlot();
            Feet = new InventoryItemSlot();
            Back = new InventoryItemSlot();
        }
        
        public InventoryItemSlot GetSlot(EGearSlot slot)
        {
            switch (slot)
            {
                case EGearSlot.Head:
                    return Head;
                    break;
                case EGearSlot.Body:
                    return Body;
                    break;
                case EGearSlot.Legs:
                    return Legs;
                    break;
                case EGearSlot.Feet:
                    return Feet;
                    break;
                case EGearSlot.Back:
                    return Back;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }
    }
}