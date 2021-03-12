using Godot;
using Game.Actor;
using Game.Loot;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class MainMenuController : GameMenu
	{
		public InventoryModel playerInventory, playerSpellBook;
		public TabContainer playerMenu, npcMenu;
		public DialogueController npcDialogue;
		public MerchantController store;
		private PopupController popup;

		public override void _Ready()
		{
			playerMenu = GetChild<TabContainer>(0);

			InventoryController inventoryController = playerMenu.GetNode<InventoryController>("Inventory/InventoryView");
			playerInventory = inventoryController.inventory;
			playerSpellBook = playerMenu.GetNode<SpellBookController>("Skills/SkillBookView").spellBook;

			npcMenu = GetChild<TabContainer>(1);

			npcDialogue = npcMenu.GetNode<DialogueController>("Dialogue/DialogueView");
			npcDialogue.closeButton.Connect("pressed", this, nameof(_OnBackPressed));

			store = npcMenu.GetNode<MerchantController>("Store/merchantView");
			store.playerInventory = playerInventory;
			store.playerSpellBook = playerSpellBook;
			store.playerInventoryGridController = inventoryController.inventorySlots;

			store.closeButton.Connect("pressed", this, nameof(_OnBackPressed));
			store.Connect(nameof(MerchantController.OnTransactionMade), inventoryController,
				nameof(InventoryController.RefreshSlots));

			popup = GetChild<PopupController>(2);
			popup.exitGameBttn.Connect("pressed", this, nameof(OnExitGamePressed));
			popup.exitMenuBttn.Connect("pressed", this, nameof(OnExitMenuPressed));

			string[] textureNames = new string[] { "chest", "power", "person", "flag", "save", "gear" };
			int i;
			for (i = 0; i < playerMenu.GetChildCount(); i++)
			{
				playerMenu.SetTabTitle(i, string.Empty);
				playerMenu.SetTabIcon(i, GD.Load<Texture>($"res://asset/img/ui/{textureNames[i]}.png"));
			}
			textureNames = new string[] { "chat", "chest" };
			for (i = 0; i < npcMenu.GetChildCount(); i++)
			{
				npcMenu.SetTabTitle(i, string.Empty);
				npcMenu.SetTabIcon(i, GD.Load<Texture>($"res://asset/img/ui/{textureNames[i]}.png"));
			}
		}
		private void OnVisibilityChanged()
		{
			GetTree().Paused = Visible;
			// resets view
			if (!Visible)
			{
				playerMenu.Show();
				npcMenu.Hide();
			}
		}
		private void OnTabChanged(int index) { PlaySound(NameDB.UI.CLICK1); }
		private void OnExitPressed() { popup.exitView.Show(); }
		private void OnExitGamePressed() { GetTree().Quit(); }
		private void OnExitMenuPressed()
		{
			PlaySound(NameDB.UI.CLICK0);
			GetTree().Paused = false;
			SceneLoaderController.Init().SetScene(PathManager.startScene, Map.Map.map);
		}
		public void NpcInteract(Npc npc)
		{
			bool showStoreMenu = Globals.contentDB.HasData(npc.Name)
					&& Globals.contentDB.GetData(npc.Name).merchandise.Length > 0,
			 showDialogueMenu = !Globals.unitDB.GetData(npc.Name).dialogue.Empty();

			npcMenu.TabsVisible = showDialogueMenu && showStoreMenu;
			npcMenu.CurrentTab = showDialogueMenu ? 0 : 1;

			Visible = npcMenu.Visible = showStoreMenu || showStoreMenu;
			npcDialogue.npc = store.merchant = npc;

			playerMenu.Visible = !Visible;
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
					QuestMaster.CheckQuests(lootChest.commodityWorldName, QuestDB.QuestType.LEARN, true);
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
					QuestMaster.CheckQuests(lootChest.commodityWorldName, QuestDB.QuestType.COLLECT, true);
					playerInventory.AddCommodity(lootChest.commodityWorldName);
					lootChest.Collect();
				}
			}
		}
	}
}