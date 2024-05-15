using System;
using System.Collections.Generic;
using Game.ItemDatabaseSystem;
using Game.Logger;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.InventorySystem
{
    [Serializable]
    public class Inventory
    {
        [ShowInInspector, TableMatrix(DrawElementMethod = nameof(DrawInventorySlots))]
        public InventoryItemSlot[,] Slots;

        public Inventory()
        {
        }
        
        public Inventory(int colLimit, int rowLimit)
        {
            Slots = new InventoryItemSlot[colLimit, rowLimit];
            for (int row = 0; row < rowLimit; row++)
                for (int col = 0; col < colLimit; col++)
                    Slots[col, row] = new InventoryItemSlot();
        }
        
        private static InventoryItemSlot DrawInventorySlots(Rect rect, InventoryItemSlot slot)
        {
            GUI.TextField(rect, slot.ItemId);
            GUI.TextField(rect, slot.Amount.ToString());
            return slot;
        }

        public bool CanGiveItem(string itemId, int amount)
        {
            BaseItemSo itemSo = null;
            ItemDatabaseHandlerData.GetItemById(itemId, ref itemSo);

            int amountToAdd = amount;
            int itemStackLimit = itemSo.StackLimit;
            int emptySlots = 0;

            // first try to add as many as we can to existing slots with same item
            for (int row = 0; row < Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Slots.GetLength(0); col++)
                {
                    if (Slots[col,row].IsEmpty)
                    {
                        emptySlots++;
                        continue;
                    }
                    if (Slots[col,row].ItemId == itemId && Slots[col,row].Amount < itemSo.StackLimit)
                    {
                        amountToAdd -= itemSo.StackLimit - Slots[col,row].Amount;
                    }
                }
            }

            if (amountToAdd == 0) return true;
            
            // how many slots do we need to take up and do we have that many empty slots?
            int amountOfSlotsNeeded = amountToAdd / itemStackLimit;
            if (amountOfSlotsNeeded <= emptySlots) return true;
            
            return false;
        }

        public int GiveItem(string itemId, int amount, out List<(int,int)> slotsModified)
        {
            int leftover = amount;
            
            BaseItemSo itemSo = null;
            ItemDatabaseHandlerData.GetItemById(itemId, ref itemSo);

            slotsModified = new List<(int, int)>();

            if (itemSo == null)
            {
                SLogger.Inventory.LogError($"Could not find itemSo with id={itemId}");
                return leftover;
            }

            int amountToAdd = amount;
            int itemStackLimit = itemSo.StackLimit;

            for (int row = 0; row < Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Slots.GetLength(0); col++)
                {
                    if (Slots[col,row].IsEmpty) continue;
                    if (Slots[col,row].ItemId == itemId && Slots[col,row].Amount < itemSo.StackLimit)
                    {
                        int freeSpace = Mathf.Min(amountToAdd, itemSo.StackLimit - Slots[col,row].Amount);
                        amountToAdd -= freeSpace;
                        Slots[col,row].Amount += freeSpace;
                        leftover -= freeSpace;
                        slotsModified.Add((col,row));
                        if (amountToAdd == 0) return leftover;
                    }
                }
            }
            
            for (int row = 0; row < Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Slots.GetLength(0); col++)
                {
                    if (Slots[col,row].IsEmpty)
                    {
                        int addAmountLimit = Mathf.Min(amountToAdd, itemStackLimit);
                        amountToAdd -= addAmountLimit;
                        leftover -= addAmountLimit;
                        Slots[col,row].Amount += addAmountLimit;
                        Slots[col,row].ItemId = itemId;
                        slotsModified.Add((col,row));
                        if (amountToAdd == 0) return leftover;
                    }
                }
            }

            return leftover;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            int amountToFind = amount;
            for (int row = 0; row < Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Slots.GetLength(0); col++)
                {
                    if (Slots[col,row].IsEmpty) continue;
                    if (Slots[col,row].ItemId == itemId)
                    {
                        amountToFind -= Slots[col,row].Amount;
                        if (amountToFind <= 0)
                            return true;
                    }
                }
            }

            return false;
        }
        
        public int GetAmount(string itemId)
        {
            int amountFound = 0;
            for (int row = 0; row < Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Slots.GetLength(0); col++)
                {
                    if (Slots[col,row].IsEmpty) continue;
                    if (Slots[col,row].ItemId == itemId)
                    {
                        amountFound += Slots[col,row].Amount;
                    }
                }
            }

            return amountFound;
        }
        
        public void TakeItem(string itemId, int amount, out List<(int,int)> slotsModified)
        {
            slotsModified = new List<(int, int)>();
            
            int amountToTake = amount;
            for (int row = 0; row < Slots.GetLength(1); row++)
            {
                for (int col = 0; col < Slots.GetLength(0); col++)
                {
                    if (Slots[col,row].IsEmpty) continue;
                    if (Slots[col,row].ItemId == itemId)
                    {
                        if (amountToTake <= Slots[col,row].Amount)
                        {
                            Slots[col,row].Amount -= amountToTake;
                            amountToTake = 0;
                            if (Slots[col,row].Amount == 0)
                                Slots[col,row].Clear();
                        }
                        else
                        {
                            amountToTake -= Slots[col,row].Amount;
                            Slots[col,row].Clear();
                        }
                        
                        slotsModified.Add((col,row));
                        if (amountToTake == 0) return;
                    }
                }
            }
        }

        public void Resize(int newSizeX, int newSizeY)
        {
            Slots = ResizeArray(Slots, newSizeX, newSizeY);
        }
        
        private InventoryItemSlot[,] ResizeArray(InventoryItemSlot[,] original, int x, int y)
        {
            InventoryItemSlot[,] newArray = new InventoryItemSlot[x, y];
            int minX = Math.Min(original.GetLength(0), newArray.GetLength(0));
            int minY = Math.Min(original.GetLength(1), newArray.GetLength(1));

            for (int i = 0; i < minY; ++i)
                Array.Copy(original, i * original.GetLength(0), newArray, i * newArray.GetLength(0), minX);
            
            // Initialize new elements
            for (int i = 0; i < x; ++i)
            {
                for (int j = minY; j < y; ++j)
                {
                    newArray[i, j] = new InventoryItemSlot();
                }
            }

            // Initialize remaining rows if necessary
            for (int i = minX; i < x; ++i)
            {
                for (int j = 0; j < y; ++j)
                {
                    newArray[i, j] = new InventoryItemSlot();
                }
            }
            
            return newArray;
        }
    }
}