using Game.Utils;
using Game.Actor;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
	public class MenuHandlerController : GameMenu
	{
		private MainMenuController mainMenuController;
		private HudStatusController hudStatusController;
		private HudControlController hudControlController;
		public override void _Ready()
		{
			GameMenu.speaker = GetNode<Speaker>("speaker");
			mainMenuController = GetNode<MainMenuController>("c/game_menu");
			mainMenuController.Connect("draw", this, nameof(_OnMainMenuDraw));
			mainMenuController.Connect("hide", this, nameof(_OnMainMenuHide));
			hudStatusController = GetNode<HudStatusController>("c/hud_status");
			hudControlController = GetNode<HudControlController>("c/hud");
			hudControlController.ConnectButtons(this, nameof(_OnHudPausePressed),
				nameof(_OnHudSpellBookPressed), nameof(_OnHudPausePressed));
		}
		public void _OnMainMenuDraw()
		{
			hudControlController.Hide();
			hudStatusController.Hide();
		}
		public void _OnMainMenuHide()
		{
			hudControlController.Show();
			hudStatusController.Show();
		}
		public void _OnHudPausePressed()
		{
			Globals.PlaySound("click5", this, speaker);
			mainMenuController.Show();
		}
		public void _OnHudSpellBookPressed()
		{
			Globals.PlaySound("click5", this, speaker);
			mainMenuController.ShowSpellBook();
		}
		public void ConnectPlayerToHud(Player player)
		{
			player.Connect(nameof(Character.UpdateHudStatus), hudStatusController, nameof(hudStatusController._OnUpdateStatus));
			hudStatusController.UpdateName(true, player.worldName);
			hudStatusController._OnUpdateStatus(player, true, player.hp, player.stats.hpMax.valueI);
			hudStatusController._OnUpdateStatus(player, false, player.mana, player.stats.manaMax.valueI);
		}
		public void ClearTarget()
		{
			hudStatusController.ClearNpcConnections();
			hudStatusController.npcStatus.Hide();
			player.target = null;
		}
		public void NpcInteract(Npc npc)
		{
			if (player.dead)
			{
				return;
			}

			string signal = nameof(Character.UpdateHudStatus);
			string method = nameof(hudStatusController._OnUpdateStatus);
			bool interactable = ContentDB.HasContent(npc.Name)
				&& 3 >= Map.Map.map.getAPath(player.GlobalPosition, npc.GlobalPosition).Count;

			if (npc.IsConnected(signal, hudStatusController, method))
			{
				if (!npc.enemy && interactable)
				{
					mainMenuController.NpcInteract(npc);
				}
				else
				{
					ClearTarget();
				}
			}
			else
			{
				hudStatusController.ClearNpcConnections();

				// connect focused npc to hud and update hud
				npc.Connect(signal, hudStatusController, method);
				hudStatusController.UpdateName(false, npc.worldName);
				hudStatusController._OnUpdateStatus(npc, true, npc.hp, npc.stats.hpMax.valueI);
				hudStatusController._OnUpdateStatus(npc, false, npc.mana, npc.stats.manaMax.valueI);

				// interact with npc
				if (!npc.enemy && interactable)
				{
					mainMenuController.NpcInteract(npc);
				}
				else
				{
					player.target = npc;
					// checks to see is play can attack
					if (npc.enemy)
					{
						player.OnAttacked(npc);
					}
				}
			}
		}
		public void LootInteract(TreasureChest lootChest)
		{
			if (player.dead)
			{
				return;
			}

			mainMenuController.LootInteract(lootChest);
		}
	}
}