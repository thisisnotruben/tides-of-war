using System.Collections.Generic;
using Game.Loot;
using Godot;
namespace Game.Ui
{
    public class ItemList : Control
    {
        public bool allowSlotsToCooldown = true;
        public byte ITEM_MAX = 25;
        [Signal]
        public delegate void OnItemSelected(int itemIdx, bool sifting);
        public override void _Ready()
        {
            for (byte i = 0; i < ITEM_MAX - GetChildCount(); i++)
            {
                PackedScene itemSlotScene = (PackedScene)GD.Load("res://src/menu_ui/item_slot.tscn");
                ItemSlot itemSlot = (ItemSlot)itemSlotScene.Instance();
                AddChild(itemSlot);
            }
            foreach (Node node in GetChildren())
            {
                ItemSlot itemSlot = node as ItemSlot;
                if (itemSlot != null)
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
            foreach (ItemSlot slot in GetUsedSlots())
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
                if (itemSlot != null && (itemSlot.GetItem() == null || (stackSlot && pickable.stackSize > 0 &&
                        pickable.Equals(itemSlot.GetItem()) && itemSlot.IsStacking() && !itemSlot.IsFull())))
                {
                    itemSlot.SetItem(pickable);
                    break;
                }
            }
            if (itemSlot != null && !itemSlot.IsCoolingDown() && allowSlotsToCooldown)
            {
                // Add cooldown to current slot, if similar items are cooling down from bag
                foreach (ItemSlot otherItemSlot in GetUsedSlots())
                {
                    Pickable otherPickable = otherItemSlot.GetItem();
                    if (pickable.Equals(otherPickable) && pickable != otherPickable && otherItemSlot.IsCoolingDown())
                    {
                        itemSlot.CoolDown(otherPickable, otherPickable.GetInitialTime(), otherPickable.GetTimeLeft());
                        return;
                    }
                }
                // Add cooldown to current slot, if similar items don't exist in bag, but in HUD
                foreach (Node node in ((InGameMenu)Owner).hpMana.GetNode("m/h/p/h/g").GetChildren())
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
            ItemSlot itemSlot = GetChild(slotIdx)as ItemSlot;
            if (itemSlot != null)
            {
                itemSlot.SetItem(null, shuffle, forceClear, funnel);
            }
        }
        public void RemoveItem(Pickable pickable, bool shuffle = true, bool forceClear = false, bool funnel = false)
        {
            RemoveItem(GetItemSlot(pickable).GetIndex(), shuffle, forceClear, funnel);
        }

        public void Clear()
        {
            foreach (ItemSlot itemSlot in GetUsedSlots())
            {
                RemoveItem(itemSlot.GetIndex(), false, true, false);
            }
        }
        public List<Pickable> GetItems(bool getFullStack)
        {
            List<Pickable> allItems = new List<Pickable>();
            foreach (ItemSlot itemSlot in GetUsedSlots())
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
            ItemSlot itemSlot = GetChild(slotIdx)as ItemSlot;
            if (itemSlot != null)
            {
                return itemSlot.GetItem();
            }
            else
            {
                GD.Print($"{Name} on method \"SetSlotCoolDown\" slotIdx out of bounds");
            }
            return null;
        }
        public void SetSlotCoolDown(int slotIdx, float value, float seek)
        {
            ItemSlot itemSlot = GetChild(slotIdx)as ItemSlot;
            if (itemSlot != null)
            {
                itemSlot.CoolDown(itemSlot.GetItem(), value, seek);
            }
        }
        public void SetSlotCoolDown(Pickable pickable, float value, float seek)
        {
            SetSlotCoolDown(GetItemSlot(pickable).GetIndex(), value, seek);
        }
        public int GetItemCount()
        {
            return GetUsedSlots().Count;
        }
        public bool IsSlotCoolingDown(int slotIdx)
        {
            ItemSlot itemSlot = GetChild(slotIdx)as ItemSlot;
            if (itemSlot != null)
            {
                return itemSlot.IsCoolingDown();
            }
            return false;
        }
        public bool IsSlotCoolingDown(Pickable pickable)
        {
            return IsSlotCoolingDown(GetItemSlot(pickable).GetIndex());
        }
        public ItemSlot GetItemSlot(Pickable pickable)
        {
            foreach (ItemSlot itemSlot in GetUsedSlots())
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
        public List<ItemSlot> GetUsedSlots()
        {
            List<ItemSlot> itemSlotList = new List<ItemSlot>();
            foreach (Node node in GetChildren())
            {
                ItemSlot itemSlot = node as ItemSlot;
                if (itemSlot != null && itemSlot.GetItem() != null)
                {
                    itemSlotList.Add(itemSlot);
                }
            }
            return itemSlotList;
        }
        public bool HasItem(Pickable pickable)
        {
            foreach (ItemSlot itemSlot in GetUsedSlots())
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