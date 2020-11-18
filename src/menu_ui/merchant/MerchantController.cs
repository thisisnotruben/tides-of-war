using Godot;
using Game.Actor;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class MerchantController : GameMenu
	{
		private readonly InventoryModel merchantStore = new InventoryModel();
		private Control merchantContent;
		private Label header;
		private Label subHeader;
		private Control toInventoryBttn;
		private Control toMerchantBttn;
		private PopupController popupController;
		private ItemInfoMerchantController itemInfoMerchantController;
		private SlotGridController merchantSlots;
		private InventoryModel _playerSpellBook;
		public InventoryModel playerSpellBook
		{
			get { return _playerSpellBook; }
			set
			{
				_playerSpellBook = value;
				itemInfoMerchantController.playerSpellBook = value;
			}
		}
		public InventoryModel playerInventory;
		public Npc merchant;

		public override void _Ready()
		{
			merchantContent = GetNode<Control>("s");

			header = merchantContent.GetNode<Label>("v/header");
			subHeader = merchantContent.GetNode<Label>("v/sub_header");

			toInventoryBttn = merchantContent.GetNode<Control>("buttons/inventory");
			toMerchantBttn = merchantContent.GetNode<Control>("buttons/merchant");

			merchantSlots = merchantContent.GetNode<SlotGridController>("v/c/SlotGrid");

			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnMerchantNodeHide));
			foreach (string nodePath in new string[] { "m/error/okay", "m/repair/back" })
			{
				popupController.GetNode<BaseButton>(nodePath).Connect(
					"pressed", this, nameof(_OnMerchantNodeHide));
			}

			// connect slot events
			foreach (SlotController slot in merchantSlots.GetSlots())
			{
				slot.button.Connect("pressed", this, nameof(OnMerchantSlotSelected),
					new Godot.Collections.Array() { slot.GetIndex() });
			}

			// set itemInfo view events
			itemInfoMerchantController = GetNode<ItemInfoMerchantController>("item_info");
			itemInfoMerchantController.Connect("hide", this, nameof(_OnMerchantNodeHide));
			itemInfoMerchantController.Connect(nameof(ItemInfoMerchantController.OnTransaction),
				this, nameof(_OnTransaction));
			itemInfoMerchantController.itemList = merchantStore;
		}
		public void _OnTransaction(string CommodityName, int goldAmount, bool bought)
		{
			// called from ItemInfo when player buys/sells
			bool isSpell = SpellDB.HasSpell(CommodityName);
			if (bought)
			{
				if (isSpell)
				{
					playerSpellBook.AddCommodity(CommodityName);
				}
				else
				{
					playerInventory.AddCommodity(CommodityName);
				}
			}
			else
			{
				if (isSpell)
				{
					playerSpellBook.RemoveCommodity(CommodityName);
				}
				else
				{
					playerInventory.RemoveCommodity(CommodityName);
				}
			}

			QuestMaster.CheckQuests(CommodityName,
				isSpell
					? QuestDB.QuestType.LEARN
					: QuestDB.QuestType.COLLECT,
				bought);

			// add/sub gold
			player.gold += goldAmount;
			subHeader.Text = "Gold: " + player.gold;
		}
		public void DisplayItems(string header, params string[] commodityNames)
		{
			this.header.Text = header;
			itemInfoMerchantController.isBuying = !header.Equals("Inventory");

			// add commodities to merchant model/view
			int i;
			for (i = 0; i < commodityNames.Length; i++)
			{
				merchantStore.AddCommodity(commodityNames[i]);
			}
			for (i = 0; i < merchantStore.count; i++)
			{
				// cannot be in same loop due to inplace-sorting && stacking
				merchantSlots.DisplaySlot(i, merchantStore.GetCommodity(i), merchantStore.GetCommodityStack(i));
			}
		}
		public void _OnMerchantNodeDraw()
		{
			ContentDB.ContentData contentData = ContentDB.GetContentData(merchant.Name);

			Globals.soundPlayer.PlaySound(
				SpellDB.HasSpell(contentData.merchandise[0])
				? NameDB.UI.TURN_PAGE
				: NameDB.UI.MERCHANT_OPEN
			);

			subHeader.Text = "Gold: " + player.gold;
		}
		public void _OnMerchantNodeHide()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.MERCHANT_CLOSE);
			popupController.Hide();
			merchantContent.Show();
		}
		public void OnMerchantSlotSelected(int slotIndex)
		{
			// don't want to click on an empty slot
			if (slotIndex >= merchantStore.count)
			{
				return;
			}

			// called from the slots in the merchant view
			string CommodityName = merchantStore.GetCommodity(slotIndex);
			bool isSpell = SpellDB.HasSpell(CommodityName);
			bool alreadyHave = false;

			if (isSpell)
			{
				Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
				Globals.soundPlayer.PlaySound(NameDB.UI.SPELL_SELECT);
				alreadyHave = playerSpellBook.HasItem(CommodityName);
			}
			else
			{
				Globals.soundPlayer.PlaySound(Database.ItemDB.GetItemData(CommodityName).material, true);
			}

			// show item details and switch view
			merchantContent.Hide();
			itemInfoMerchantController.selectedSlotIdx = slotIndex;
			itemInfoMerchantController.Display(CommodityName, true,
				!header.Text.Equals("Inventory"), alreadyHave);
		}
		public void _OnMerchantPressed()
		{
			// switches to what the merchant is selling view
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
			toInventoryBttn.Show();
			toMerchantBttn.Hide();

			// display to what merchant is selling
			merchantSlots.ClearSlots();
			DisplayItems(merchant.worldName,
				ContentDB.GetContentData(merchant.Name).merchandise);
		}
		public void _OnInventoryPressed()
		{
			// switches to what the player is selling view
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
			toInventoryBttn.Hide();
			toMerchantBttn.Show();

			// display to what player is selling
			merchantSlots.ClearSlots();
			DisplayItems("Inventory", playerInventory.GetCommodities().ToArray());
		}
		public override void _OnBackPressed()
		{
			base._OnBackPressed();
			merchantStore.Clear();
			merchantSlots.ClearSlots();
		}
	}
}