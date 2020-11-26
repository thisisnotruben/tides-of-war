using Game.Actor;
using Game.Quest;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
	public class MenuHandlerController : GameMenu
	{
		public MainMenuController mainMenuController;
		private HudControlController hudControlController;

		public override void _Ready()
		{
			mainMenuController = GetNode<MainMenuController>("c/game_menu");
			mainMenuController.Connect("draw", this, nameof(_OnMainMenuDraw));
			mainMenuController.Connect("hide", this, nameof(_OnMainMenuHide));
			hudControlController = GetNode<HudControlController>("c/hud");
			hudControlController.ConnectButtons(this, nameof(_OnHudPausePressed),
				nameof(_OnHudSpellBookPressed), nameof(_OnHudPausePressed));

			mainMenuController.inventoryController.itemInfoInventoryController
				.popupController.addToSlotView.Connect("draw", this, nameof(_OnMainMenuHide));
			mainMenuController.inventoryController.itemInfoInventoryController
				.popupController.addToSlotView.Connect("hide", this, nameof(_OnMainMenuDraw));

			mainMenuController.statsController.itemInfoController
				.popupController.addToSlotView.Connect("draw", this, nameof(_OnMainMenuHide));
			mainMenuController.statsController.itemInfoController
				.popupController.addToSlotView.Connect("hide", this, nameof(_OnMainMenuDraw));
		}
		public void _OnMainMenuDraw() { hudControlController.Hide(); }
		public void _OnMainMenuHide() { hudControlController.Show(); }
		public void _OnHudPausePressed()
		{
			PlaySound(NameDB.UI.CLICK5);
			mainMenuController.Show();
		}
		public void _OnHudSpellBookPressed()
		{
			PlaySound(NameDB.UI.CLICK5);
			mainMenuController.ShowSpellBook();
		}
		public void ConnectPlayerToHud(Player player)
		{
			hudControlController.playerStatus.ConnectCharacterStatusAndUpdate(player);
		}
		public void SetTargetDisplay(Npc target)
		{
			if (!hudControlController.targetStatus.IsCharacterConnected(target))
			{
				hudControlController.targetStatus.ConnectCharacterStatusAndUpdate(target);
			}
		}
		public void ClearTarget()
		{
			player.target = null;
			hudControlController.targetStatus.Clear(false);
		}
		public void NpcInteract(Npc npc)
		{
			if (player.dead)
			{
				return;
			}

			bool interactable = !npc.enemy &&
				(ContentDB.Instance.HasData(npc.Name) || QuestMaster.HasQuestOrQuestExtraContent(npc.worldName, npc.GetPath()))
				&& 3 >= Map.Map.map.getAPath(player.GlobalPosition, npc.GlobalPosition).Count;

			if (hudControlController.targetStatus.IsCharacterConnected(npc))
			{
				if (interactable)
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
				SetTargetDisplay(npc);

				// interact with npc
				if (interactable)
				{
					mainMenuController.NpcInteract(npc);
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