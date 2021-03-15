using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;
using Game.GameItem;
using Game.Database;
namespace Game.Ui
{
	public class SlotGridController : GameMenu, ISerializable
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
		public void ClearSlot(int slotIndex)
		{
			List<int> slotsTemp = new List<int>();
			foreach (int s in slotModelMap.Keys)
			{
				if (slotModelMap[s] > slotModelMap[slotIndex])
				{
					slotsTemp.Add(s);
				}
			}

			slotsTemp.ForEach(s => slotModelMap[s]--);
			slots[slotIndex].ClearDisplay();
			slotModelMap.Remove(slotIndex);
		}
		public void ClearSlots()
		{
			slotModelMap.Clear();
			slots.ForEach(s => s.ClearDisplay());
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
		public GC.Dictionary Serialize()
		{
			GC.Dictionary payload = new GC.Dictionary();
			foreach (int key in slotModelMap.Keys)
			{
				payload[key] = new GC.Dictionary()
					{
						{NameDB.SaveTag.INDEX, slotModelMap[key]},
						{NameDB.SaveTag.SLOT, slots[key].Serialize()}
					};
			}
			return payload;
		}
		public void Deserialize(GC.Dictionary payload)
		{
			GC.Dictionary packet;
			int s;
			foreach (string key in payload.Keys)
			{
				packet = (GC.Dictionary)payload[key];
				s = key.ToInt();

				slotModelMap[s] = packet[NameDB.SaveTag.INDEX].ToString().ToInt();
				slots[s].Deserialize((GC.Dictionary)packet[NameDB.SaveTag.SLOT]);
			}
		}
	}
}