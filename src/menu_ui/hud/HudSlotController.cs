using Game.Database;
using Game.GameItem;
using Godot;
using System.Linq;
namespace Game.Ui
{
	public class HudSlotController : SlotController
	{
		public override void _Ready()
		{
			base._Ready();
			AddToGroup(Globals.HUD_SHORTCUT_GROUP);
		}
		public void Display(string worldName, NodePath characterPath, InventoryModel inventoryModel)
		{
			int legalAmount = PickableDB.GetStackSize(worldName),
				amount = (from commodityName in inventoryModel.GetCommodities()
						  where worldName.Equals(commodityName)
						  select commodityName).Count();

			ClearDisplay();
			Display(worldName, amount > legalAmount ? legalAmount : amount);

			SetCooldown(Commodity.GetCoolDown(characterPath, worldName));
		}
		public void ShowAddToHudDisplay(string displayText)
		{
			tween.StopAll();
			coolDownText.Text = displayText;
			cooldownOverlay.Visible = coolDownText.Visible = true;
		}
		public void RevertAddToHudDisplay()
		{
			if (IsAvailable())
			{
				ClearDisplay();
			}
			else
			{
				tween.SetActive(coolDownActive);
				cooldownOverlay.Visible = coolDownText.Visible = coolDownActive;
			}
		}
	}
}