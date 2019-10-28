using Godot;
using Game.Misc.Loot;
using System.Collections.Generic;

namespace Game.Ui
{
    public class ItemList : Control
    {
        public bool allowSlotsToCooldown;
        public ushort ITEM_MAX;

        [Signal]
        public delegate void OnItemSelected(int itemIdx, bool sifting);

        public override void _Ready()
        {
            for (ushort i = 0; i < ITEM_MAX - GetChildCount(); i++)
            {
                PackedScene itemSlotScene = (PackedScene)GD.Load("res://src/menu_ui/item_slot.tscn");
                ItemSlot itemSlot = (ItemSlot)itemSlotScene.Instance();
                AddChild(itemSlot);
            }
            foreach (Node node in GetChildren())
            {
                ItemSlot itemSlot = node as ItemSlot;
                if (node != null)
                {
                    itemSlot.Connect(nameof(ItemSlot.SlotSelected), this, nameof(_OnSlotSelected));
                    itemSlot.Connect(nameof(ItemSlot.StackSizeChanged), this, nameof(_OnStackSizeChanged));
                    foreach (Node otherNode in GetChildren())
                    {
                        ItemSlot otherItemSlot = otherNode as ItemSlot;
                        if (otherItemSlot != null && otherItemSlot != itemSlot)
                        {
                            itemSlot.Connect(nameof(ItemSlot.Cooldown), otherItemSlot, nameof(ItemSlot.CoolDown));
                        }
                    }
                }
            }
        }
        public void _OnSlotSelected(int slotIdx)
        {
            EmitSignal(nameof(OnItemSelected), slotIdx, false);
        }
        public void _OnStackSizeChanged(string worldName, int slotSize, ItemSlot itemSlot)
        {
            foreach (ItemSlot slot in GetUsedSlots(false))
            {
                if (itemSlot.GetItem().Equals(slot.GetItem()) && !itemSlot.IsFull())
                {
                    itemSlot.SetItem(slot.GetItem());
                    RemoveItem(slot.GetIndex(), false, false, true);
                    break;
                }
            }
        }
        public void AddItem(Pickable pickable, bool stackSlot)
        {
            ItemSlot itemSlot = null;
            foreach (Node node in GetChildren())
            {
                itemSlot = node as ItemSlot;
                if ((itemSlot != null) && ((itemSlot.GetItem() != null) || (stackSlot && pickable.GetStackSize() > 0
                && pickable.Equals(itemSlot.GetItem()) && itemSlot.IsStacking() && !itemSlot.IsFull())))
                {
                    itemSlot.SetItem(pickable);
                    break;
                }
            }
            if (allowSlotsToCooldown && itemSlot != null && !itemSlot.IsCoolingDown())
            {
                // Add cooldown to current slot, if similar items are cooling down from bag
                foreach (ItemSlot otherItemSlot in GetUsedSlots(true))
                {
                    Pickable otherPickable = otherItemSlot.GetItem();
                    if (pickable.Equals(otherPickable) && pickable != otherPickable && otherItemSlot.IsCoolingDown())
                    {
                        itemSlot.CoolDown(otherPickable, otherPickable.GetInitialTime(), otherPickable.GetTimeLeft());
                        return;
                    }
                }
                // Add cooldown to current slot, if similar items don't exist in bag, but in HUD
                foreach (Node node in ((InGameMenu)GetOwner()).hpMana.GetNode("m/h/p/h/g").GetChildren())
                {
                    ItemSlot hudItemSlot = node as ItemSlot;
                    if (hudItemSlot != null && hudItemSlot.GetItem() != null && pickable.Equals(hudItemSlot.GetItem()))
                    {
                        Pickable otherPickable = hudItemSlot.GetItem();
                        itemSlot.CoolDown(otherPickable, otherPickable.GetInitialTime(), otherPickable.GetTimeLeft());
                        return;
                    }
                }
            }
        }
        public void RemoveItem(int slotIdx, bool shuffle = true, bool forceClear = false, bool funnel = false)
        {
            ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
            if (itemSlot != null)
            {
                itemSlot.SetItem(null, shuffle, forceClear, funnel);
            }
            else
            {
                GD.PrintErr($"{GetName()} on method \"RemoveItem\" slotIdx out of bounds");
            }
        }
        public void Clear()
        {
            foreach (ItemSlot itemSlot in GetUsedSlots(true))
            {
                RemoveItem(itemSlot.GetIndex(), false, true, false);
            }
        }
        public List<Pickable> GetItems(bool getFullStack)
        {
            List<Pickable> allItems = new List<Pickable>();
            foreach (ItemSlot itemSlot in GetUsedSlots(true))
            {
                foreach (Pickable pickable in itemSlot.GetItemStack())
                {
                    allItems.Add(pickable);
                }
            }
            return allItems;
        }
        public Pickable GetItemMetaData(int slotIdx)
        {
            ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
            if (itemSlot != null)
            {
                return itemSlot.GetItem();
            }
            else
            {
                GD.PrintErr($"{GetName()} on method \"SetSlotCoolDown\" slotIdx out of bounds");
            }
            return null;
        }
        public void SetSlotCoolDown(int slotIdx, float value, float seek)
        {
            ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
            if (itemSlot != null)
            {
                itemSlot.CoolDown(itemSlot.GetItem(), value, seek);
            }
            else
            {
                GD.PrintErr($"{GetName()} on method \"SetSlotCoolDown\" slotIdx out of bounds");
            }
        }
        public int GetItemCount()
        {
            return GetUsedSlots(true).Count;
        }
        public bool IsSlotCoolingDown(int slotIdx)
        {
            ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
            if (itemSlot != null)
            {
                return itemSlot.IsCoolingDown();
            }
            else
            {
                GD.PrintErr($"{GetName()} on method \"IsSlotCoolingDown\" slotIdx out of bounds");
            }
            return false;
        }
        public ItemSlot GetItemSlot(Pickable pickable, bool index = false)
        {
            foreach (ItemSlot itemSlot in GetUsedSlots(false))
            {
                foreach (Pickable otherPickable in itemSlot.GetItemStack())
                {
                    if (pickable == otherPickable)
                    {
                        return itemSlot;
                    }
                }
            }
            return null;
        }
        public List<ItemSlot> GetUsedSlots(bool includeStackedSlots)
        {
            List<ItemSlot> itemSlotList = new List<ItemSlot>();
            foreach (Node node in GetChildren())
            {
                ItemSlot itemSlot = node as ItemSlot;
                if (itemSlot != null && itemSlot.GetItem() != null)
                {
                    if (!includeStackedSlots || (includeStackedSlots && itemSlot.IsStacking()))
                    {
                        itemSlotList.Add(itemSlot);
                    }
                }
            }
            return itemSlotList;
        }
        public bool HasItem(Pickable pickable)
        {
            foreach (ItemSlot itemSlot in GetUsedSlots(true))
            {
                foreach (Pickable otherPickable in itemSlot.GetItemStack())
                {
                    if (otherPickable == pickable)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsFull()
        {
            return GetItemCount() >= ITEM_MAX;
        }
    }
}