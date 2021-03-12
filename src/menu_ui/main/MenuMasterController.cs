using Godot;
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
		public HudPopupConfirmController confirmPopup;
		public HudPopupErrorController errorPopup;
		private CanvasItem hudMenuContainer, menuContainer;
		private SaveLoadModel saveLoadModel;

		public override void _Ready()
		{
			menuContainer = GetNode<CanvasItem>("canvasLayer/split");
			gameMenu = menuContainer.GetChild<MainMenuController>(0);

			saveLoadModel = gameMenu.playerMenu.GetNode<SaveLoadController>("Save Load/SaveLoadView").saveLoadModel;

			hudMenuContainer = menuContainer.GetChild<CanvasItem>(1);
			hudMenuContainer.Connect("visibility_changed", this, nameof(OnHudMenuVisibilityChanged));

			confirmPopup = menuContainer.GetChild<HudPopupConfirmController>(2);
			errorPopup = menuContainer.GetChild<HudPopupErrorController>(3);

			hud = menuContainer.GetChild<HudControlController>(4);
			hud.pause.Connect("toggled", this, nameof(OnHudPausePressed));
			hud.targetContainer.Connect("hide", this, nameof(OnNpcCloseHud));

			ItemInfoHudController itemInfoHudController = hudMenuContainer.GetNode<ItemInfoHudController>("Inventory/itemInfo");
			itemInfoHudController.inventoryModel = gameMenu.playerInventory;
			itemInfoHudController.slotGridController = gameMenu.GetNode<InventoryController>("playerMenu/Inventory/InventoryView").inventorySlots;

			ItemInfoHudSpellController infoHudSpellController = hudMenuContainer.GetNode<ItemInfoHudSpellController>("Spells/itemInfo");
			infoHudSpellController.inventoryModel = gameMenu.playerSpellBook;
			infoHudSpellController.slotGridController = gameMenu.GetNode<SpellBookController>("playerMenu/Skills/SkillBookView").spellSlots;

			itemInfoHudController.tabContainer = infoHudSpellController.tabContainer = hudMenuContainer;

			infoHudSpellController.Connect(nameof(ItemInfoHudSpellController.PlayerWantstoCastError),
				errorPopup, nameof(HudPopupErrorController.ShowError));
		}
		private void OnHudPausePressed(bool toggled)
		{
			PlaySound(NameDB.UI.CLICK5);
			gameMenu.Visible = toggled;
			if (toggled)
			{
				// just in case the player wants to save
				saveLoadModel.SetCurrentGameImage();

				HideExceptMenu(gameMenu);
				gameMenu.npcMenu.Hide();
				gameMenu.playerMenu.Show();
			}
		}
		private void OnHudMenuVisibilityChanged()
		{
			GetTree().Paused = hudMenuContainer.Visible;
			if (hudMenuContainer.Visible)
			{
				HideExceptMenu(hudMenuContainer);
			}
		}
		private void OnNpcCloseHud()
		{
			if (gameMenu.npcMenu.Visible)
			{
				HideExceptMenu(null);
			}
		}
		private void HideExceptMenu(CanvasItem hideExcept)
		{
			CanvasItem menu;
			foreach (Node node in menuContainer.GetChildren())
			{
				menu = node as CanvasItem;
				if (menu != null && menu != hud && menu != hideExcept)
				{
					menu.Hide();
				}
			}
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
					HideExceptMenu(gameMenu);
					gameMenu.NpcInteract(npc);
				}
				else
				{
					ClearTarget();
				}
			}
			else
			{
				SetTargetDisplay(npc);

				if (interactable)
				{
					HideExceptMenu(gameMenu);
					gameMenu.NpcInteract(npc);
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