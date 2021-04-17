using System.Collections.Generic;
using Game.Database;
namespace Game.Ui
{
	public class InventoryModel
	{
		public class Slot
		{
			public string commodityName;
			public int stackSize;

			public Slot(string commodityName, int stackSize)
			{
				this.commodityName = commodityName;
				this.stackSize = stackSize;
			}
		}
		private List<Slot> slots = new List<Slot>();
		public int count { get { return slots.Count; } }
		public int maxSlots = 30;

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
		public int AddCommodity(string commodityWorldName)
		{
			if (IsFull(commodityWorldName))
			{
				return -1;
			}

			int commodityStackSize = PickableDB.GetStackSize(commodityWorldName);

			for (int i = count - 1; i >= 0; i--)
			{
				// check if commodity already exists in inventory for stacking
				if (slots[i].commodityName.Equals(commodityWorldName)
				&& slots[i].stackSize < commodityStackSize)
				{
					slots[i].stackSize += 1;
					return i;
				}
			}

			slots.Add(new Slot(commodityWorldName, 1));
			return count - 1;
		}
		public bool PushCommodity(string commodityWorldName, int stack)
		{
			if (IsFull(commodityWorldName))
			{
				return false;
			}
			slots.Add(new Slot(commodityWorldName, stack));
			return true;
		}
		public int RemoveCommodity(string commodityName)
		{
			for (int i = 0; i < count; i++)
			{
				if (slots[i].commodityName.Equals(commodityName))
				{
					slots[i].stackSize -= 1;

					// remove if stack depleted
					if (slots[i].stackSize == 0)
					{
						slots.RemoveAt(i);
						return i;
					}
					break;
				}
			}
			return -1;
		}
		public bool RemoveCommodity(int index)
		{
			slots[index].stackSize -= 1;
			if (slots[index].stackSize <= 0)
			{
				slots.RemoveAt(index);
				return true;
			}
			return false;
		}
		public void Clear() { slots.Clear(); }
		public bool IsFull() { return maxSlots == count; }
		public bool IsFull(string worldName)
		{
			int commodityStackSize = PickableDB.GetStackSize(worldName);

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