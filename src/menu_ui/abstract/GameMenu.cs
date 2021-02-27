using Godot;
using Game.Actor;
using Game.Database;
namespace Game.Ui
{
	public abstract class GameMenu : Control
	{
		public static Player player;

		public void PlaySound(string soundName) { Globals.soundPlayer.PlaySound(soundName); }
		public void CheckHudSlots(InventoryModel inventoryModel, string commodityWorldName)
		{
			HudSlotController hudSlotController;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				hudSlotController = node as HudSlotController;
				if (hudSlotController?.HasCommodity(commodityWorldName) ?? false)
				{
					if (inventoryModel.HasItem(commodityWorldName))
					{
						// update stack or cooldown if needed
						hudSlotController.Display(commodityWorldName, player.GetPath(), inventoryModel);
					}
					else
					{
						hudSlotController.ClearDisplay();
					}
				}
			}
		}
		public virtual void _OnBackPressed()
		{
			PlaySound(NameDB.UI.CLICK3);
			Hide();
		}
	}
}