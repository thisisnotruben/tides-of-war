using System.Collections.Generic;
using Godot;
using Game.Database;
namespace Game.Ui
{
	public class ItemList : Control
	{
		public bool allowSlotsToCooldown = true;
		public byte ITEM_MAX = 25;
		[Signal]
		public delegate void OnItemSelected(int itemIdx);
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
			EmitSignal(nameof(OnItemSelected), slotIdx);
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
		public void AddItem(string pickableWorldName, bool stackSlot)
		{
			ItemSlot itemSlot = null;
			foreach (Node node in GetChildren())
			{
				itemSlot = node as ItemSlot;
				if (itemSlot != null && (itemSlot.GetItem().Empty()
				|| (stackSlot && PickableDB.GetStackSize(pickableWorldName) > 0
				&& pickableWorldName.Equals(itemSlot.GetItem()) && itemSlot.IsStacking() && !itemSlot.IsFull())))
				{
					itemSlot.SetItem(pickableWorldName);
					break;
				}
			}
			// TODO
			// if (itemSlot != null && !itemSlot.IsCoolingDown() && allowSlotsToCooldown)
			// {
			// Add cooldown to current slot, if similar items are cooling down from bag
			// foreach (ItemSlot otherItemSlot in GetUsedSlots())
			// {
			//     string otherPickableWorldName = otherItemSlot.GetItem();
			//     if (pickableWorldName.Equals(otherPickableWorldName)
			//     && pickableWorldName != otherPickableWorldName && otherItemSlot.IsCoolingDown())
			//     {
			//         itemSlot.CoolDown(otherPickableWorldName, otherPickable.GetInitialTime(), otherPickable.GetTimeLeft());
			//         return;
			//     }
			// }
			// Add cooldown to current slot, if similar items don't exist in bag, but in HUD
			// foreach (Node node in ((InGameMenu)Owner).hpMana.GetNode("m/h/p/h/g").GetChildren())
			// {
			//     ItemSlot hudItemSlot = node as ItemSlot;
			//     if (hudItemSlot != null && hudItemSlot.GetItem() != null && pickable.Equals(hudItemSlot.GetItem()))
			//     {
			//         Pickable otherPickable = hudItemSlot.GetItem();
			//         itemSlot.CoolDown(otherPickable, otherPickable.GetInitialTime(), otherPickable.GetTimeLeft());
			//         return;
			//     }
			// }
			// }
		}
		public void RemoveItem(int slotIdx, bool shuffle = true, bool forceClear = false, bool funnel = false)
		{
			ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
			if (itemSlot != null)
			{
				itemSlot.SetItem(null, shuffle, forceClear, funnel);
			}
		}
		public void RemoveItem(string pickableWorldName, bool shuffle = true, bool forceClear = false, bool funnel = false)
		{
			RemoveItem(GetItemSlot(pickableWorldName).GetIndex(), shuffle, forceClear, funnel);
		}
		public void Clear()
		{
			foreach (ItemSlot itemSlot in GetUsedSlots())
			{
				RemoveItem(itemSlot.GetIndex(), false, true, false);
			}
		}
		public List<string> GetItems(bool getFullStack)
		{
			List<string> allItems = new List<string>();
			foreach (ItemSlot itemSlot in GetUsedSlots())
			{
				foreach (string pickableWorldName in itemSlot.GetItemStack())
				{
					allItems.Add(pickableWorldName);
				}
			}
			return allItems;
		}
		public string GetItemMetaData(int slotIdx)
		{
			ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
			return (itemSlot != null) ? itemSlot.GetItem() : "";
		}
		public void SetSlotCoolDown(int slotIdx, float value, float seek)
		{
			ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
			if (itemSlot != null)
			{
				itemSlot.CoolDown(itemSlot.GetItem(), value, seek);
			}
		}
		public void SetSlotCoolDown(string pickableWorldName, float value, float seek)
		{
			SetSlotCoolDown(GetItemSlot(pickableWorldName).GetIndex(), value, seek);
		}
		public int GetItemCount()
		{
			return GetUsedSlots().Count;
		}
		public bool IsSlotCoolingDown(int slotIdx)
		{
			ItemSlot itemSlot = GetChild(slotIdx) as ItemSlot;
			if (itemSlot != null)
			{
				return itemSlot.IsCoolingDown();
			}
			return false;
		}
		public bool IsSlotCoolingDown(string pickableWorldName)
		{
			return IsSlotCoolingDown(GetItemSlot(pickableWorldName).GetIndex());
		}
		public ItemSlot GetItemSlot(string pickableWorldName)
		{
			foreach (ItemSlot itemSlot in GetUsedSlots())
			{
				foreach (string otherPickableWorldName in itemSlot.GetItemStack())
				{
					if (pickableWorldName == otherPickableWorldName)
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
				if (itemSlot != null && !itemSlot.GetItem().Empty())
				{
					itemSlotList.Add(itemSlot);
				}
			}
			return itemSlotList;
		}
		public bool HasItem(string pickableWorldName, bool identity = true)
		{
			foreach (ItemSlot itemSlot in GetUsedSlots())
			{
				foreach (string otherPickableWorldName in itemSlot.GetItemStack())
				{
					if ((identity && pickableWorldName == otherPickableWorldName)
					|| (!identity && pickableWorldName.Equals(otherPickableWorldName)))
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