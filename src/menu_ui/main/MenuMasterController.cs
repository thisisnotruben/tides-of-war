using Godot;
using GC = Godot.Collections;
using Game.Actor;
using Game.Database;
namespace Game.Ui
{
	public class MenuMasterController : GameMenu
	{
		public MainMenuController playerMenu;
		private NpcMenu npcMenu;
		public HudControlController hud;
		public HudPopupConfirmController confirmPopup;
		public HudPopupErrorController errorPopup;
		public SlotGridController inventorySlots, spellSlots;
		private CanvasItem hudMenuContainer, menuContainer, loadView;
		private SaveLoadModel saveLoadModel;

		public override void _Ready()
		{
			menuContainer = GetNode<CanvasItem>("canvasLayer/split");
			playerMenu = menuContainer.GetChild<MainMenuController>(0);
			inventorySlots = playerMenu.GetNode<InventoryController>("playerMenu/Inventory/InventoryView").inventorySlots;
			spellSlots = playerMenu.GetNode<SpellBookController>("playerMenu/Skills/SkillBookView").spellSlots;

			saveLoadModel = playerMenu.GetNode<SaveLoadController>("playerMenu/Save Load/SaveLoadView").saveLoadModel;

			hudMenuContainer = menuContainer.GetChild<CanvasItem>(1);
			hudMenuContainer.Connect("visibility_changed", this, nameof(OnHudMenuVisibilityChanged));

			confirmPopup = menuContainer.GetNode<HudPopupConfirmController>("confirmPopup");
			errorPopup = menuContainer.GetNode<HudPopupErrorController>("errorPopup");

			loadView = menuContainer.GetNode<CanvasItem>("loadView");
			Globals.sceneLoader.progressBar = loadView.GetNode<Range>(
				"panelContainer/marginContainer/vBoxContainer/progressBar");

			npcMenu = menuContainer.GetNode<NpcMenu>("npcMenu").Init(
				this, playerMenu.inventoryController, playerMenu.spellBookController,
				playerMenu.playerInventory, playerMenu.playerSpellBook, saveLoadModel);
			npcMenu.store.Connect("draw", this, nameof(HideExceptMenu), new GC.Array() { npcMenu });

			npcMenu.Connect(nameof(NpcMenu.QuestStarted), playerMenu.questLogController,
				nameof(QuestLogController.AddEntry));

			hud = menuContainer.GetNode<HudControlController>("hud");
			hud.targetContainer.Connect("hide", this, nameof(OnTargetCleared));
			hud.pause.Connect("toggled", this, nameof(OnHudPausePressed));

			ItemInfoHudController itemInfoHudController = hudMenuContainer.GetNode<ItemInfoHudController>("Inventory/itemInfo");
			itemInfoHudController.inventoryModel = playerMenu.playerInventory;
			itemInfoHudController.slotGridController = inventorySlots;
			playerMenu.Connect("draw", itemInfoHudController, nameof(ItemInfoHudController.OnGameMenuVisibilityChanged), new GC.Array() { true });
			playerMenu.Connect("hide", itemInfoHudController, nameof(ItemInfoHudController.OnGameMenuVisibilityChanged), new GC.Array() { false });

			ItemInfoHudSpellController infoHudSpellController = hudMenuContainer.GetNode<ItemInfoHudSpellController>("Spells/itemInfo");
			infoHudSpellController.inventoryModel = playerMenu.playerSpellBook;
			infoHudSpellController.slotGridController = spellSlots;
			playerMenu.Connect("draw", infoHudSpellController, nameof(ItemInfoHudSpellController.OnGameMenuVisibilityChanged), new GC.Array() { true });
			playerMenu.Connect("hide", infoHudSpellController, nameof(ItemInfoHudSpellController.OnGameMenuVisibilityChanged), new GC.Array() { false });

			itemInfoHudController.tabContainer = infoHudSpellController.tabContainer = hudMenuContainer;

			infoHudSpellController.Connect(nameof(ItemInfoHudSpellController.PlayerWantstoCastError),
				errorPopup, nameof(HudPopupErrorController.ShowError));
		}
		private void OnHudPausePressed(bool toggled)
		{
			PlaySound(NameDB.UI.CLICK5);

			if (!playerMenu.Visible)
			{
				// just in case the player wants to save
				saveLoadModel.SetCurrentGameImage();
			}

			playerMenu.Visible = toggled;
			if (toggled)
			{
				HideExceptMenu(playerMenu);
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
		public void ShowLoadView()
		{
			HideExceptMenu(null);
			loadView.Show();
		}
		public void ShowTransitionDialogue(CanvasItem dialogic)
		{
			menuContainer.AddChild(dialogic);
			menuContainer.MoveChild(dialogic, 0);
			HideExceptMenu(dialogic);
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
		private void OnTargetCleared()
		{
			npcMenu.Hide();
			if (!playerMenu.Visible)
			{
				GetTree().Paused = false;
			}
		}
		public void NpcInteract(Npc npc) { npcMenu.NpcInteract(npc); }
		public void LootInteract(ICollectable collectable, string itemName) { playerMenu.LootInteract(collectable, itemName); }
	}
}