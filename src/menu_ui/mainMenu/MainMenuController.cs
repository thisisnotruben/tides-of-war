using Godot;
using Game.Actor;
using Game.Loot;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class MainMenuController : GameMenu
	{
		private Control main, background;
		private ColorRect overlay;

		public InventoryController inventoryController;
		public StatsController statsController;
		private QuestLogController questLogController;
		private AboutController aboutController;
		private SaveLoadController saveLoadController;
		public SpellBookController spellBookController;
		private DialogueController dialogueController;
		private PopupController popupController;
		public InventoryModel playerInventory, playerSpellBook;

		public override void _Ready()
		{
			main = GetNode<Control>("background/margin/main");

			inventoryController = GetNode<InventoryController>("background/margin/inventory");
			playerInventory = inventoryController.inventory;

			statsController = GetNode<StatsController>("background/margin/stats");
			questLogController = GetNode<QuestLogController>("background/margin/quest_log");
			aboutController = GetNode<AboutController>("background/margin/about");
			saveLoadController = GetNode<SaveLoadController>("background/margin/save_load");

			PopupController.mainMenuController = this;
			popupController = GetNode<PopupController>("background/margin/popup");
			popupController.exitGameBttn.Connect("pressed", this, nameof(_OnExitGamePressed));
			popupController.exitMenuBttn.Connect("pressed", this, nameof(_OnExitMenuPressed));

			spellBookController = GetNode<SpellBookController>("background/margin/spell_book");
			spellBookController.Connect("hide", this, nameof(_OnMainMenuHide));
			playerSpellBook = spellBookController.spellBook;

			dialogueController = GetNode<DialogueController>("background/margin/dialogue");
			dialogueController.Connect("hide", this, nameof(_OnMainMenuHide));
			dialogueController.Init(playerInventory, playerSpellBook);

			// route hide events
			foreach (Control node in new Control[] { inventoryController, statsController,
			questLogController, aboutController, saveLoadController, popupController })
			{
				node.Connect("hide", this, nameof(_OnWindowClosed));
			}

			background = GetNode<Control>("background");
			overlay = GetNode<ColorRect>("overlay");
		}
		public void _OnWindowClosed() { main.Show(); }
		public void _OnMainMenuDraw() { GetTree().Paused = true; } // connected from scene
		public void _OnMainMenuHide() // connected from scene
		{
			Hide();
			GetTree().Paused = false;
			main.Show();
			popupController.Hide();
		}
		public void ShowSpellBook()
		{
			main.Hide();
			spellBookController.Show();
			Show();
		}
		public void ShowBackground(bool show)
		{
			Color transparent = new Color("00ffffff");
			background.SelfModulate = show
				? new Color("ffffff")
				: transparent;
			overlay.Color = show
				? new Color("6e6e6e")
				: transparent;
		}
		public void NpcInteract(Npc npc)
		{
			main.Hide();
			dialogueController.Show();
			dialogueController.Display(npc);
			Show();
		}
		public void LootInteract(TreasureChest lootChest)
		{
			if (SpellDB.Instance.HasData(lootChest.commodityWorldName))
			{
				if (playerSpellBook.IsFull(lootChest.commodityWorldName))
				{
					popupController.ShowError("Spell Book\nFull!");
				}
				else
				{
					QuestMaster.CheckQuests(lootChest.commodityWorldName, QuestDB.QuestType.LEARN, true);
					playerSpellBook.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}
			else
			{
				if (playerInventory.IsFull(lootChest.commodityWorldName))
				{
					popupController.ShowError("Inventory\nFull!");
				}
				else
				{
					QuestMaster.CheckQuests(lootChest.commodityWorldName, QuestDB.QuestType.COLLECT, true);
					playerInventory.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}

		}
		public void _OnResumePressed()
		{
			PlaySound(NameDB.UI.CLICK2);
			Hide();
		}
		public void _OnInventoryPressed() { Transition(inventoryController); }
		public void _OnStatsPressed() { Transition(statsController); }
		public void _OnQuestLogPressed() { Transition(questLogController); }
		public void _OnAboutPressed() { Transition(aboutController); }
		public void _OnSaveLoadPressed() { Transition(saveLoadController); }
		public void _OnExitPressed()
		{
			popupController.exitView.Show();
			Transition(popupController);
		}
		public void _OnExitGamePressed() { GetTree().Quit(); }
		public void _OnExitMenuPressed()
		{
			PlaySound(NameDB.UI.CLICK0);
			GetTree().Paused = false;
			SceneLoaderController.Init().SetScene(PathManager.startScene, Map.Map.map);
		}
		private void Transition(Control scene)
		{
			PlaySound(NameDB.UI.CLICK1);
			main.Hide();
			scene.Show();
		}
	}
}