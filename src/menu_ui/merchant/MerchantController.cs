using Godot;
using Game.Actor;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class MerchantController : GameMenu
	{
		[Signal] public delegate void OnTransactionMade();

		private readonly InventoryModel store = new InventoryModel();
		private Control mainContent;
		private Label header;
		public Button buySellButton, closeButton;
		private ItemInfoMerchantController storeItemInfo;
		public SlotGridController storeSlots, playerInventoryGridController;
		private InventoryModel _playerSpellBook;
		public InventoryModel playerSpellBook
		{
			get { return _playerSpellBook; }
			set
			{
				_playerSpellBook = value;
				storeItemInfo.playerSpellBook = value;
			}
		}
		public InventoryModel playerInventory;
		private Npc _merchant;
		public Npc merchant
		{
			get { return _merchant; }
			set
			{
				_merchant = value;
				// resets store
				buySellButton.Text = "Sell";
				storeItemInfo.Visible = buySellButton.Pressed = false;
				mainContent.Show();
				if (value != null)
				{
					DisplayItems(Globals.contentDB.GetData(value.Name).merchandise);
				}
			}
		}

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);
			header = mainContent.GetChild<Label>(0);
			storeSlots = mainContent.GetNode<SlotGridController>("c/SlotGrid");
			closeButton = mainContent.GetNode<Button>("gridContainer/close");
			buySellButton = mainContent.GetNode<Button>("gridContainer/buySell");
			buySellButton.Connect("toggled", this, nameof(OnBuySellToggled));
			foreach (SlotController slot in storeSlots.GetSlots())
			{
				slot.allowDrag = false;
				slot.button.Connect("pressed", this, nameof(OnStoreSlotSelected),
					new Godot.Collections.Array() { slot.GetIndex() });
			}

			storeItemInfo = GetChild<ItemInfoMerchantController>(1);
			storeItemInfo.backBttn.Connect("pressed", this, nameof(OnItemStoreInfoBackPressed));
			storeItemInfo.Connect("hide", this, nameof(OnItemStoreInfoBackPressed));
			storeItemInfo.Connect(nameof(ItemInfoMerchantController.OnTransaction), this, nameof(OnTransaction));
			storeItemInfo.inventoryModel = store;
			storeItemInfo.slotGridController = storeSlots;
		}
		private void OnTransaction(string commodityName, int goldAmount, bool bought)
		{
			// called from ItemInfo when player buys/sells
			bool isSpell = Globals.spellDB.HasData(commodityName);
			if (bought)
			{
				(isSpell ? playerSpellBook : playerInventory).AddCommodity(commodityName);
			}
			else
			{
				store.RemoveCommodity(commodityName);
				if (isSpell)
				{
					playerSpellBook.RemoveCommodity(commodityName);
				}
				else
				{
					int modelIndex = playerInventory.RemoveCommodity(commodityName);
					if (modelIndex != -1)
					{
						playerInventoryGridController.ClearSlot(modelIndex);
						storeSlots.ClearSlot(modelIndex);
					}
				}
			}

			QuestMaster.CheckQuests(commodityName,
				isSpell
					? QuestDB.QuestType.LEARN
					: QuestDB.QuestType.COLLECT,
				bought
			);

			// add/sub gold
			player.gold += goldAmount;
			header.Text = "Gold: " + player.gold;
			EmitSignal(nameof(OnTransactionMade));
		}
		private void DisplayItems(params string[] commodityNames)
		{
			store.Clear();
			storeSlots.ClearSlots();
			storeItemInfo.isBuying = !buySellButton.Pressed;

			// add commodities to merchant model/view
			for (int i = 0; i < commodityNames.Length; i++)
			{
				store.AddCommodity(commodityNames[i]);
			}

			// cannot be in same loop due to inplace-sorting && stacking
			int m;
			for (int s = 0; s < storeSlots.GetSlots().Count; s++)
			{
				m = s;
				if (buySellButton.Pressed)
				{
					// mimic the look of player inventory
					if (playerInventoryGridController.IsSlotUsed(s))
					{
						m = playerInventoryGridController.GetSlotToModelIndex(s);
					}
					else
					{
						continue;
					}
				}
				else if (s >= store.count)
				{
					break;
				}
				storeSlots.DisplaySlot(s, m, store.GetCommodity(m), store.GetCommodityStack(m));
			}
		}
		private void OnDraw() { header.Text = "Gold: " + (player?.gold ?? 0); }
		private void OnItemStoreInfoBackPressed() { mainContent.Show(); }
		private void OnStoreSlotSelected(int slotIndex)
		{
			// don't want to click on an empty slot

			if (!storeSlots.IsSlotUsed(slotIndex))
			{
				return;
			}

			// called from the slots in the merchant view
			string CommodityName = store.GetCommodity(storeSlots.GetSlotToModelIndex(slotIndex));
			bool isSpell = Globals.spellDB.HasData(CommodityName),
				alreadyHave = false;

			if (isSpell)
			{
				PlaySound(NameDB.UI.CLICK1);
				PlaySound(NameDB.UI.SPELL_SELECT);
				alreadyHave = playerSpellBook.HasItem(CommodityName);
			}
			else
			{
				Globals.soundPlayer.PlaySound(Globals.itemDB.GetData(CommodityName).material, true);
			}

			// show item details and switch view
			mainContent.Hide();
			storeItemInfo.selectedSlotIdx = slotIndex;
			storeItemInfo.Display(CommodityName, true, !buySellButton.Pressed, alreadyHave);
		}
		private void OnBuySellToggled(bool buttonPressed)
		{
			PlaySound(NameDB.UI.CLICK1);
			buySellButton.Text = buttonPressed ? "Buy" : "Sell";
			if (player != null && merchant != null)
			{
				DisplayItems(
					buttonPressed
						? playerInventory.GetCommodities().ToArray()
						: Globals.contentDB.GetData(merchant.Name).merchandise
				);
			}
		}
	}
}