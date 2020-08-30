using System.Collections.Generic;
using Game.Database;
using Game.ItemPoto;
using Game.Actor;
namespace Game.UI
{
	public class Inventory
	{
		public struct Slot
		{
			public string commodityName;
			public List<Commodity> commodities;
		}
		public int maxSlots;
		public List<Slot> slots = new List<Slot>();
		private Character character;

		public Inventory(int maxSlots)
		{
			this.maxSlots = maxSlots;
		}
		private int SortSlots(Slot a, Slot b)
		{
			if (a.commodities.Count < b.commodities.Count)
			{
				return -1;
			}
			if (a.commodities.Count > b.commodities.Count)
			{
				return 1;
			}
			return 0;
		}
		public Commodity GetCommodity(int index)
		{
			if (index > maxSlots || index < 0)
			{
				return null;
			}
			return slots[index].commodities[0];
		}
		public bool AddCommodity(Commodity commodity)
		{
			if (IsFull(commodity.worldName))
			{
				return false;
			}

			int commodityStackSize = ItemDB.GetItemData(commodity.worldName).stackSize;
			for (int i = slots.Count - 1; i >= 0; i--)
			{
				// check if commodity already exists in inventory for stacking
				if (slots[i].commodityName.Equals(commodity.worldName)
				&& slots[i].commodities.Count < commodityStackSize)
				{
					slots[i].commodities.Add(commodity);
					return true;
				}
			}

			// add new slot
			Slot slot;
			slot.commodityName = commodity.worldName;
			slot.commodities = new List<Commodity>() { commodity };

			slots.Add(slot);
			slots.Sort(SortSlots);
			return true;
		}
		public bool RemoveCommodity(Commodity commodity, bool alike)
		{
			bool removedItem = false;
			int i, j;
			for (i = slots.Count - 1; i >= 0 & !removedItem; i--)
			{
				if (!slots[i].commodityName.Equals(commodity.worldName))
				{
					continue;
				}

				if (alike)
				{
					// remove an alike commodity
					slots[i].commodities.RemoveAt(0);
					removedItem = true;
				}
				else
				{
					// remove specific commodity
					for (j = 0; j < slots[i].commodities.Count && !removedItem; j++)
					{
						if (slots[i].commodities[j] == commodity)
						{
							slots[i].commodities.RemoveAt(j);
							removedItem = true;
						}
					}
				}
			}

			// remove slot if empty
			if (removedItem && slots[i].commodities.Count == 0)
			{
				slots.Remove(slots[i]);
			}

			return removedItem;
		}
		public void Clear() { slots.Clear(); }
		public bool IsFull() { return maxSlots == slots.Count; }
		public bool IsFull(string worldName)
		{
			int commodityStackSize = ItemDB.GetItemData(worldName).stackSize;

			// inventory might be full, but there might be some stack space
			if (IsFull())
			{
				for (int i = slots.Count - 1; i >= 0; i--)
				{
					if (slots[i].commodityName.Equals(worldName) && slots[i].commodities.Count < commodityStackSize)
					{
						return false;
					}
				}
			}
			return true;
		}
		public bool HasItem(Commodity commodity, bool alike)
		{
			foreach (Slot slot in slots)
			{
				if (!slot.commodityName.Equals(commodity.worldName))
				{
					continue;
				}

				if (alike)
				{
					return true;
				}
				else
				{
					foreach (Commodity stackedCommodity in slot.commodities)
					{
						if (stackedCommodity == commodity)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}