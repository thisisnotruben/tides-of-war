using Game.Actor;
using Game.Quest;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
	public class MenuMasterController : GameMenu
	{
		public MainMenuController gameMenu;
		private HudControlController hud;

		public override void _Ready()
		{
			gameMenu = GetNode<MainMenuController>("canvasLayer/split/gameMenu");
			hud = GetNode<HudControlController>("canvasLayer/split/hud");

			hud.pause.Connect("toggled", this, nameof(OnHudPausePressed));
		}
		public void OnHudPausePressed(bool toggled)
		{
			PlaySound(NameDB.UI.CLICK5);
			gameMenu.Visible = toggled;

		}
		public void ConnectPlayerToHud(Player player) { hud.playerStatus.ConnectCharacterStatusAndUpdate(player); }
		public void SetTargetDisplay(Npc target)
		{
			if (!hud.targetStatus.IsCharacterConnected(target))
			{
				hud.targetStatus.ConnectCharacterStatusAndUpdate(target);
			}
		}
		public void ClearTarget()
		{
			player.target = null;
			hud.targetStatus.Clear(false);
		}
		public void NpcInteract(Npc npc)
		{
			if (player.dead)
			{
				return;
			}

			bool interactable = !npc.enemy &&
				(Globals.contentDB.HasData(npc.Name) || QuestMaster.HasQuestOrQuestExtraContent(npc.worldName, npc.GetPath()))
				&& 3 >= Map.Map.map.getAPath(player.GlobalPosition, npc.GlobalPosition).Count;

			if (hud.targetStatus.IsCharacterConnected(npc))
			{
				if (interactable)
				{
					hud.pause.Pressed = gameMenu.NpcInteract(npc);
				}
				else
				{
					ClearTarget();
				}
			}
			else
			{
				SetTargetDisplay(npc);

				// interact with npc
				if (interactable)
				{
					hud.pause.Pressed = gameMenu.NpcInteract(npc);
				}
				else
				{
					player.target = npc;
					// checks to see if play can attack
					if (npc.enemy)
					{
						player.OnAttacked(npc);
					}
				}
			}
		}
		public void LootInteract(TreasureChest lootChest) { gameMenu.LootInteract(lootChest); }
	}
}