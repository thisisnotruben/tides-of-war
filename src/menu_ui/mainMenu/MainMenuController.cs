using Godot;
using Game.Database;
namespace Game.Ui
{
	public class MainMenuController : GameMenu
	{
		public InventoryController inventoryController;
		public SpellBookController spellBookController;
		public InventoryModel playerInventory, playerSpellBook;
		public QuestLogController questLogController;
		public StatsController statsController;
		private TabContainer playerMenu;
		private PopupController popup;

		public override void _Ready()
		{
			playerMenu = GetChild<TabContainer>(0);

			inventoryController = playerMenu.GetNode<InventoryController>("Inventory/InventoryView");
			spellBookController = playerMenu.GetNode<SpellBookController>("Skills/SkillBookView");
			playerInventory = inventoryController.inventory;
			playerSpellBook = spellBookController.spellBook;

			questLogController = playerMenu.GetNode<QuestLogController>("Quest Log/QuestLogView");
			statsController = playerMenu.GetNode<StatsController>("Stats/StatsView");

			popup = GetChild<PopupController>(1);
			GetNode<BaseButton>("playerMenu/Settings/centerContainer/vBoxContainer/exitGame").Connect(
				"pressed", this, nameof(CheckExit), new Godot.Collections.Array() { true });
			GetNode<BaseButton>("playerMenu/Settings/centerContainer/vBoxContainer/exitMenu").Connect(
				"pressed", this, nameof(CheckExit), new Godot.Collections.Array() { false });

			string[] textureNames = new string[] { "chest", "power", "person", "flag", "save", "gear" };
			for (int i = 0; i < playerMenu.GetChildCount(); i++)
			{
				playerMenu.SetTabTitle(i, string.Empty);
				playerMenu.SetTabIcon(i, GD.Load<Texture>(
					string.Format(PathManager.menuIconPath, textureNames[i])));
			}
		}
		private void OnVisibilityChanged()
		{
			GetTree().Paused = Visible;
			if (Visible)
			{
				statsController.OnDraw();
			}
		}
		private void OnTabChanged(int index) { PlaySound(NameDB.UI.CLICK1); }
		private void ExitGame() { GetTree().Quit(); }
		private void ExitMenu()
		{
			PlaySound(NameDB.UI.CLICK0);
			Globals.cooldownMaster.ClearCooldowns();
			Globals.sceneLoader.SetScene(PathManager.startScene, Map.Map.map);
		}
		private void CheckExit(bool exitGame)
		{
			if (SaveLoadModel.dirty)
			{
				popup.RouteConnection(exitGame ? nameof(ExitGame)
					: nameof(ExitMenu), this);
				popup.ShowConfirm("Lose Save\nData?");
			}
			else if (exitGame)
			{
				ExitGame();
			}
			else
			{
				ExitMenu();
			}
		}
		public void LootInteract(ICollectable collectable, string itemName)
		{
			if (player.dead)
			{
				return;
			}

			if (Globals.spellDB.HasData(itemName))
			{
				if (playerSpellBook.IsFull(itemName))
				{
					popup.ShowError("Spell Book\nFull!");
				}
				else
				{
					Globals.questMaster.CheckQuests(itemName, QuestDB.QuestType.LEARN, true);
					playerSpellBook.AddCommodity(itemName);
					collectable.Collect();
				}
			}
			else if (Globals.itemDB.HasData(itemName))
			{
				if (playerInventory.IsFull(itemName))
				{
					popup.ShowError("Inventory\nFull!");
				}
				else
				{
					Globals.questMaster.CheckQuests(itemName, QuestDB.QuestType.COLLECT, true);
					playerInventory.AddCommodity(itemName);
					collectable.Collect();
				}
			}
		}
	}
}