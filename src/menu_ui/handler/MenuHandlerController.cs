using Game.Utils;
using Game.Actor;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
	public class MenuHandlerController : GameMenu
	{
		private MainMenuController mainMenuController;
		private HudControlController hudControlController;

		public override void _Ready()
		{
			GameMenu.speaker = GetNode<Speaker>("speaker");
			mainMenuController = GetNode<MainMenuController>("c/game_menu");
			mainMenuController.Connect("draw", this, nameof(_OnMainMenuDraw));
			mainMenuController.Connect("hide", this, nameof(_OnMainMenuHide));
			hudControlController = GetNode<HudControlController>("c/hud");
			hudControlController.ConnectButtons(this, nameof(_OnHudPausePressed),
				nameof(_OnHudSpellBookPressed), nameof(_OnHudPausePressed));
		}
		public void _OnMainMenuDraw()
		{
			hudControlController.Hide();
		}
		public void _OnMainMenuHide()
		{
			hudControlController.Show();
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
			hudControlController.playerStatus.ConnectCharacterStatusAndUpdate(player);
		}
		public void ClearTarget(Npc playerTarget = null)
		{
			playerTarget?.unitFocus.Hide();
			player.target = null;
			hudControlController.targetStatus.Clear(false);
		}
		public void NpcInteract(Npc npc)
		{
			if (player.dead)
			{
				return;
			}

			bool interactable = ContentDB.HasContent(npc.Name)
				&& 3 >= Map.Map.map.getAPath(player.GlobalPosition, npc.GlobalPosition).Count;

			if (hudControlController.targetStatus.IsCharacterConnected(npc))
			{
				if (!npc.enemy && interactable)
				{
					mainMenuController.NpcInteract(npc);
				}
				else
				{
					ClearTarget(npc);
				}
			}
			else
			{
				// clear previous focus
				if (player.target != null)
				{
					(player.target as Npc)?.unitFocus.Hide();
				}

				npc.unitFocus.Show();

				// connect focused npc to hud and update hud
				hudControlController.targetStatus.ConnectCharacterStatusAndUpdate(npc);

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