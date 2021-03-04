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
			SlotController hudSlot;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				hudSlot = node as SlotController;
				if (hudSlot?.HasCommodity(commodityWorldName) ?? false)
				{
					if (inventoryModel.HasItem(commodityWorldName))
					{
						// TODO: update stack or cooldown if needed
						// hudSlot.Display(commodityWorldName, 1);
					}
					else
					{
						hudSlot.ClearDisplay();
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