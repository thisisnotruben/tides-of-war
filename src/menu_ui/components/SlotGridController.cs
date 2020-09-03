using System.Collections.Generic;
using Godot;
namespace Game.Ui
{
	public class SlotGridController : GridContainer
	{
		private readonly List<SlotController> slots = new List<SlotController>();

		public override void _Ready()
		{
			SlotController slotController;
			foreach (Control control in GetChildren())
			{
				slotController = control as SlotController;
				if (slotController != null)
				{
					slots.Add(slotController);
				}
			}
		}
		public List<SlotController> GetSlots() { return slots; }
		public void DisplaySlot(int index, string commodityName, int currentStackSize) { slots[index].Display(commodityName, currentStackSize); }
		public void ClearSlots()
		{
			foreach (SlotController slot in slots)
			{
				slot.ClearDisplay();
			}
		}
	}
}