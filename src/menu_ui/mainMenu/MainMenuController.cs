using Godot;
using Game.Loot;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class MainMenuController : GameMenu
	{
		public InventoryController inventoryController;
		public SpellBookController spellBookController;
		public InventoryModel playerInventory, playerSpellBook;
		private TabContainer playerMenu;
		private PopupController popup;

		public override void _Ready()
		{
			playerMenu = GetChild<TabContainer>(0);

			inventoryController = playerMenu.GetNode<InventoryController>("Inventory/InventoryView");
			spellBookController = playerMenu.GetNode<SpellBookController>("Skills/SkillBookView");
			playerInventory = inventoryController.inventory;
			playerSpellBook = spellBookController.spellBook;

			popup = GetChild<PopupController>(1);
			GetNode<BaseButton>("playerMenu/Settings/centerContainer/vBoxContainer/exitGame").Connect(
				"pressed", this, nameof(CheckExit), new Godot.Collections.Array() { true });
			GetNode<BaseButton>("playerMenu/Settings/centerContainer/vBoxContainer/exitMenu").Connect(
				"pressed", this, nameof(CheckExit), new Godot.Collections.Array() { false });

			string[] textureNames = new string[] { "chest", "power", "person", "flag", "save", "gear" };
			for (int i = 0; i < playerMenu.GetChildCount(); i++)
			{
				playerMenu.SetTabTitle(i, string.Empty);
				playerMenu.SetTabIcon(i, GD.Load<Texture>($"res://asset/img/ui/{textureNames[i]}.png"));
			}
		}
		private void OnVisibilityChanged() { GetTree().Paused = Visible; }
		private void OnTabChanged(int index) { PlaySound(NameDB.UI.CLICK1); }
		private void ExitGame() { GetTree().Quit(); }
		private void ExitMenu()
		{
			PlaySound(NameDB.UI.CLICK0);
			Globals.cooldownMaster.ClearCooldowns();
			SceneLoaderController.Init().SetScene(PathManager.startScene, Map.Map.map);
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
		public void LootInteract(TreasureChest lootChest)
		{
			if (player.dead)
			{
				return;
			}

			if (Globals.spellDB.HasData(lootChest.commodityWorldName))
			{
				if (playerSpellBook.IsFull(lootChest.commodityWorldName))
				{
					popup.ShowError("Spell Book\nFull!");
				}
				else
				{
					Globals.questMaster.CheckQuests(lootChest.commodityWorldName, QuestDB.QuestType.LEARN, true);
					playerSpellBook.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}
			else
			{
				if (playerInventory.IsFull(lootChest.commodityWorldName))
				{
					popup.ShowError("Inventory\nFull!");
				}
				else
				{
					Globals.questMaster.CheckQuests(lootChest.commodityWorldName, QuestDB.QuestType.COLLECT, true);
					playerInventory.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}
		}
	}
}