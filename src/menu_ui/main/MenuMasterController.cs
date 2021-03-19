using Godot;
using GC = Godot.Collections;
using Game.Actor;
using Game.Quest;
using Game.Loot;
using Game.Database;
using Game.Actor.State;
using System;
namespace Game.Ui
{
	public class MenuMasterController : GameMenu
	{
		public MainMenuController gameMenu;
		private HudControlController hud;
		public HudPopupConfirmController confirmPopup;
		public HudPopupErrorController errorPopup;
		public SlotGridController inventorySlots, spellSlots;
		private CanvasItem hudMenuContainer, menuContainer;
		private SaveLoadModel saveLoadModel;

		public override void _Ready()
		{
			menuContainer = GetNode<CanvasItem>("canvasLayer/split");
			gameMenu = menuContainer.GetChild<MainMenuController>(0);
			inventorySlots = gameMenu.GetNode<InventoryController>("playerMenu/Inventory/InventoryView").inventorySlots;
			spellSlots = gameMenu.GetNode<SpellBookController>("playerMenu/Skills/SkillBookView").spellSlots;

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
			itemInfoHudController.slotGridController = inventorySlots;
			gameMenu.Connect("draw", itemInfoHudController, nameof(ItemInfoHudController.OnGameMenuVisibilityChanged), new GC.Array() { true });
			gameMenu.Connect("hide", itemInfoHudController, nameof(ItemInfoHudController.OnGameMenuVisibilityChanged), new GC.Array() { false });

			ItemInfoHudSpellController infoHudSpellController = hudMenuContainer.GetNode<ItemInfoHudSpellController>("Spells/itemInfo");
			infoHudSpellController.inventoryModel = gameMenu.playerSpellBook;
			infoHudSpellController.slotGridController = spellSlots;
			gameMenu.Connect("draw", infoHudSpellController, nameof(ItemInfoHudSpellController.OnGameMenuVisibilityChanged), new GC.Array() { true });
			gameMenu.Connect("hide", infoHudSpellController, nameof(ItemInfoHudSpellController.OnGameMenuVisibilityChanged), new GC.Array() { false });

			itemInfoHudController.tabContainer = infoHudSpellController.tabContainer = hudMenuContainer;

			infoHudSpellController.Connect(nameof(ItemInfoHudSpellController.PlayerWantstoCastError),
				errorPopup, nameof(HudPopupErrorController.ShowError));
		}
		private void OnHudPausePressed(bool toggled)
		{
			PlaySound(NameDB.UI.CLICK5);

			if (!gameMenu.Visible)
			{
				// just in case the player wants to save
				saveLoadModel.SetCurrentGameImage();
			}

			gameMenu.Visible = toggled;
			if (toggled)
			{
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
		public void SetTargetDisplay(Character target)
		{
			if (target != null && !hud.targetStatus.IsCharacterConnected(target))
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

			Action showNpcMenu = () =>
			{
				saveLoadModel.SetCurrentGameImage();
				HideExceptMenu(gameMenu);
				gameMenu.NpcInteract(npc);
			};

			bool interactable = !npc.enemy &&
				(Globals.contentDB.HasData(npc.Name) || QuestMaster.HasQuestOrQuestExtraContent(npc.worldName, npc.GetPath()))
				&& 3 >= Map.Map.map.getAPath(player.GlobalPosition, npc.GlobalPosition).Count;

			if (hud.targetStatus.IsCharacterConnected(npc))
			{
				if (interactable)
				{
					showNpcMenu();
				}
				else
				{
					ClearTarget();
				}
			}
			else
			{
				SetTargetDisplay(npc);

				player.target = npc;
				if (npc.enemy && player.pos.DistanceTo(npc.pos) <= player.stats.weaponRange.value)
				{
					player.state = FSM.State.ATTACK;
				}
				else if (interactable)
				{
					showNpcMenu();
				}
			}
		}
		public void LootInteract(TreasureChest lootChest) { gameMenu.LootInteract(lootChest); }
	}
}