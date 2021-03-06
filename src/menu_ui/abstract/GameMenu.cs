using Godot;
using Game.Actor;
using Game.Database;
using Game.GameItem;
using System.Linq;
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
						hudSlot.Display(commodityWorldName,
							PickableDB.GetStackSize(commodityWorldName) > 1
							? (from commodityName in inventoryModel.GetCommodities()
							   where commodityWorldName.Equals(commodityName)
							   select commodityName).Count()
							: 1);
						hudSlot.SetCooldown(Commodity.GetCoolDown(player.GetPath(), commodityWorldName));
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