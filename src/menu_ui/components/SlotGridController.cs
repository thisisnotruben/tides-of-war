using System.Collections.Generic;
using Godot;
using Game.GameItem;
namespace Game.Ui
{
	public class SlotGridController : GameMenu
	{
		private readonly List<SlotController> slots = new List<SlotController>();
		private readonly Dictionary<int, int> slotModelMap = new Dictionary<int, int>();

		public override void _Ready()
		{
			SlotController slotController;
			foreach (Control control in GetChildren())
			{
				slotController = control as SlotController;
				if (slotController != null)
				{
					slots.Add(slotController);
					slotController.Connect(nameof(SlotController.OnSlotDragMoved), this, nameof(OnSlotMove));
				}
			}
		}
		public List<SlotController> GetSlots() { return slots; }
		public void DisplaySlot(int slotIndex, int modelIndex, string commodityName, int currentStackSize)
		{
			slots[slotIndex].Display(commodityName, currentStackSize);
			slotModelMap[slotIndex] = modelIndex;
		}
		public void DisplaySlot(int slotIndex, int modelSlot, string commodityName, int currentStackSize, float coolDown)
		{
			DisplaySlot(slotIndex, modelSlot, commodityName, currentStackSize);
			slots[slotIndex].SetCooldown(coolDown);
		}
		public void ClearSlot(int index)
		{
			if (index >= 0)
			{
				slots[index].ClearDisplay();
				slotModelMap.Remove(index);
			}
		}
		public void ClearSlots()
		{
			for (int i = 0; i < slots.Count; i++)
			{
				ClearSlot(i);
			}
		}
		public bool IsSlotUsed(int slotIndex) { return slotModelMap.ContainsKey(slotIndex); }
		public bool IsModelSlotUsed(int modelIndex) { return slotModelMap.ContainsValue(modelIndex); }
		public int GetSlotToModelIndex(int slotIndex) { return slotModelMap[slotIndex]; }
		public void RefreshSlots(InventoryModel inventoryModel)
		{
			string slotItemName;
			foreach (int s in slotModelMap.Keys)
			{
				slotItemName = slots[s].commodityWorldName;

				slots[s].Display(slotItemName, inventoryModel.GetCommodityStack(slotModelMap[s]));
				slots[s].SetCooldown(Commodity.GetCoolDown(player.GetPath(), slotItemName));
			}
		}
		public int GetNextSlot(int slotIndex, bool forward, bool usedSlot = true)
		{
			while ((!forward && --slotIndex >= 0) || (forward && ++slotIndex < slots.Count))
			{
				if ((usedSlot && !slots[slotIndex].IsAvailable())
				|| (!usedSlot && slots[slotIndex].IsAvailable()))
				{
					return slotIndex;
				}
			}
			return -1;
		}
		private void OnSlotMove(string itemName, NodePath slotFrom, NodePath slotTo)
		{
			int slotIndexFrom = GetNode(slotFrom).GetIndex();

			slotModelMap[GetNode(slotTo).GetIndex()] = slotModelMap[slotIndexFrom];
			slotModelMap.Remove(slotIndexFrom);
		}
	}
}