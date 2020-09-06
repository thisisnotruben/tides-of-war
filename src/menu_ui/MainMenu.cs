using Godot;
using Game.Actor;
using Game.Loot;
using Game.Database;
namespace Game.Ui
{
	public class MainMenu : GameMenu
	{
		private Control main;
		private InventoryController inventoryController;
		private StatsController statsController;
		private QuestLogController questLogController;
		private AboutController aboutController;
		private SaveLoadController saveLoadController;
		private SpellBookController spellBookController;
		private DialogueController dialogueController;
		private PopupController popupController;
		private InventoryModel playerInventory;
		private InventoryModel playerSpellBook;

		public override void _Ready()
		{
			main = GetNode<Control>("background/margin/main");

			inventoryController = GetNode<InventoryController>("background/margin/inventory");
			playerInventory = inventoryController.inventory;

			statsController = GetNode<StatsController>("background/margin/stats");
			questLogController = GetNode<QuestLogController>("background/margin/quest_log");
			aboutController = GetNode<AboutController>("background/margin/about");
			saveLoadController = GetNode<SaveLoadController>("background/margin/save_load");

			popupController = GetNode<PopupController>("background/margin/popup");
			PopupController.mainMenu = this;
			popupController.GetNode<BaseButton>("m/exit/exit_game")
				.Connect("pressed", this, nameof(_OnExitGamePressed));
			popupController.GetNode<BaseButton>("m/exit/exit_menu")
				.Connect("pressed", this, nameof(_OnExitMenuPressed));

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
		}
		public void _OnWindowClosed() { main.Show(); }
		public void _OnMainMenuDraw() { GetTree().Paused = true; }
		public void _OnMainMenuHide()
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
			GetNode<Control>("background").SelfModulate =
				(show) ? new Color("ffffff") : transparent;
			GetNode<ColorRect>("overlay").Color = (show) ? new Color("6e6e6e") : transparent;
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
			if (SpellDB.HasSpell(lootChest.commodityWorldName))
			{
				if (playerSpellBook.IsFull(lootChest.commodityWorldName))
				{
					popupController.GetNode<Label>("m/error/label").Text = "Spell Book\nFull!";
					popupController.GetNode<Control>("m/error").Show();
					popupController.Show();
				}
				else
				{
					playerSpellBook.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}
			else
			{
				if (playerInventory.IsFull(lootChest.commodityWorldName))
				{
					popupController.GetNode<Label>("m/error/label").Text = "Inventory\nFull!";
					popupController.GetNode<Control>("m/error").Show();
					popupController.Show();
				}
				else
				{
					playerInventory.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}

		}
		public void _OnResumePressed()
		{
			Globals.PlaySound("click2", this, speaker);
			Hide();
		}
		public void _OnInventoryPressed() { Transition(inventoryController); }
		public void _OnStatsPressed() { Transition(statsController); }
		public void _OnQuestLogPressed() { Transition(questLogController); }
		public void _OnAboutPressed() { Transition(aboutController); }
		public void _OnSaveLoadPressed() { Transition(saveLoadController); }
		public void _OnExitPressed()
		{
			popupController.GetNode<Control>("m/exit").Show();
			Transition(popupController);
		}
		public void _OnExitGamePressed() { GetTree().Quit(); }
		public void _OnExitMenuPressed()
		{
			Globals.PlaySound("click0", this, speaker);
			GetTree().Paused = false;
			SceneLoader.Init().SetScene(
				(string)ProjectSettings.GetSetting("application/run/main_scene"),
				Map.Map.map);
		}
		private void Transition(Control scene)
		{
			Globals.PlaySound("click1", this, speaker);
			main.Hide();
			scene.Show();
		}
	}
}