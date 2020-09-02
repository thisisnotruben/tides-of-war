using System.Collections.Generic;
using Game.Database;
namespace Game.Ui
{
	public class InventoryModel
	{
		public struct Slot
		{
			public string commodityName;
			public int stackSize;
		}
		private List<Slot> slots = new List<Slot>();
		public int count { get { return slots.Count; } }
		public int maxSlots = 25;

		private int SortSlots(Slot a, Slot b)
		{
			if (a.stackSize > b.stackSize)
			{
				return -1;
			}
			if (a.stackSize < b.stackSize)
			{
				return 1;
			}
			return 0;
		}
		public string GetCommodity(int index) { return slots[index].commodityName; }
		public int GetCommodityStack(int index) { return slots[index].stackSize; }
		public List<string> GetCommodities(bool stack = true)
		{
			List<string> commodities = new List<string>();
			int i, j;
			for (i = 0; i < count; i++)
			{
				if (stack)
				{
					// add all stack
					for (j = 0; j < slots[i].stackSize; j++)
					{
						commodities.Add(slots[i].commodityName);
					}
				}
				else
				{
					commodities.Add(GetCommodity(i));
				}
			}
			return commodities;
		}
		public bool AddCommodity(string commodityWorldName)
		{
			if (IsFull(commodityWorldName))
			{
				return false;
			}

			int commodityStackSize = ItemDB.GetItemData(commodityWorldName).stackSize;
			for (int i = count - 1; i >= 0; i--)
			{
				// check if commodity already exists in inventory for stacking
				if (slots[i].commodityName.Equals(commodityWorldName)
				&& slots[i].stackSize < commodityStackSize)
				{
					// cannot modify struct slot directly in loop
					Slot focusedSlot = slots[i];
					focusedSlot.stackSize += 1;
					slots[i] = focusedSlot;

					slots.Sort(SortSlots);
					return true;
				}
			}

			// make new slot
			Slot slot;
			slot.commodityName = commodityWorldName;
			slot.stackSize = 1;

			// add slot
			slots.Add(slot);
			slots.Sort(SortSlots);

			return true;
		}
		public bool RemoveCommodity(string commodityName)
		{
			for (int i = 0; i < count; i++)
			{
				if (slots[i].commodityName.Equals(commodityName))
				{
					// cannot modify directly
					Slot focusedSlot = slots[i];
					focusedSlot.stackSize -= 1;

					// remove if stack depleted
					if (slots[i].stackSize == 0)
					{
						slots.RemoveAt(i);
					}
					return true;
				}
			}
			return false;
		}
		public void Clear() { slots.Clear(); }
		public bool IsFull() { return maxSlots == count; }
		public bool IsFull(string worldName)
		{
			int commodityStackSize = ItemDB.GetItemData(worldName).stackSize;

			// inventory might be full, but there might be some stack space
			if (IsFull())
			{
				for (int i = count - 1; i >= 0; i--)
				{
					if (slots[i].commodityName.Equals(worldName) && slots[i].stackSize < commodityStackSize)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		public bool HasItem(string commodityName)
		{
			foreach (Slot slot in slots)
			{
				if (slot.commodityName.Equals(commodityName))
				{
					return true;
				}
			}
			return false;
		}
	}
}