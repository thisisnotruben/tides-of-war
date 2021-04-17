using Godot;
using Game.Actor;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class MerchantController : GameMenu
	{
		[Signal] public delegate void OnTransactionMade();

		// EXTERNAL
		private Button sellBuyButton;
		private InventoryModel playerSpellBook, playerInventory;
		private SlotGridController playerInventorySlots, playerSpellBookSlots;

		// INTERNAL
		private readonly InventoryModel store = new InventoryModel();
		private SlotGridController storeSlots;
		private Control mainContent;
		private Label header;
		private ItemInfoMerchantController storeItemInfo;
		private Npc _merchant;
		public Npc merchant
		{
			get { return _merchant; }
			set
			{
				_merchant = value;
				// resets store
				storeItemInfo.Visible = false;
				mainContent.Show();
				if (value != null)
				{
					DisplayItemsNpc(Globals.contentDB.GetData(value.Name).merchandise);
				}
			}
		}

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);
			header = mainContent.GetNode<Label>("label");

			storeSlots = mainContent.GetNode<SlotGridController>("c/store");
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
		public MerchantController Init(InventoryModel playerInventory, InventoryController inventoryController,
		InventoryModel playerSpellBook, SpellBookController spellBookController, Button sellBuyButton)
		{
			this.playerInventory = playerInventory;
			this.playerSpellBook = storeItemInfo.playerSpellBook = playerSpellBook;
			this.playerInventorySlots = inventoryController.inventorySlots;
			this.playerSpellBookSlots = spellBookController.spellSlots;
			this.sellBuyButton = sellBuyButton;
			this.sellBuyButton.Connect("toggled", this, nameof(OnBuySellToggled));

			Connect(nameof(OnTransactionMade), inventoryController, nameof(InventoryController.RefreshSlots));
			Connect(nameof(OnTransactionMade), spellBookController, nameof(SpellBookController.RefreshSlots));

			return this;
		}
		private void OnTransaction(string commodityName, int goldAmount, bool bought, int slotIndex)
		{
			// called from ItemInfo when player buys/sells
			bool isSpell = Globals.spellDB.HasData(commodityName);
			if (bought)
			{
				(isSpell ? playerSpellBook : playerInventory).AddCommodity(commodityName);
			}
			else
			{
				store.RemoveCommodity(storeSlots.GetSlotToModelIndex(slotIndex));
				if (isSpell)
				{
					playerSpellBook.RemoveCommodity(commodityName);
					playerSpellBookSlots.ClearSlot(slotIndex);
					storeSlots.ClearSlot(slotIndex);
				}
				else
				{
					if (playerInventory.RemoveCommodity(playerInventorySlots.GetSlotToModelIndex(slotIndex)))
					{
						playerInventorySlots.ClearSlot(slotIndex);
						storeSlots.ClearSlot(slotIndex);
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
		private void DisplayItemsPlayer()
		{
			store.Clear();
			storeSlots.ClearSlots();
			storeItemInfo.isBuying = false;

			for (int s = 0, m = 0; s < storeSlots.GetSlots().Count; s++)
			{
				if (s < playerInventory.count)
				{
					store.PushCommodity(playerInventory.GetCommodity(s),
						playerInventory.GetCommodityStack(s));
				}
				if (playerInventorySlots.IsSlotUsed(s))
				{
					m = playerInventorySlots.GetSlotToModelIndex(s);
					storeSlots.DisplaySlot(s, m, store.GetCommodity(m), store.GetCommodityStack(m));
				}
			}
		}
		private void DisplayItemsNpc(params string[] commodityNames)
		{
			store.Clear();
			storeSlots.ClearSlots();
			storeItemInfo.isBuying = true;

			for (int i = 0; i < commodityNames.Length && i < storeSlots.GetSlots().Count; i++)
			{
				store.AddCommodity(commodityNames[i]);
				storeSlots.DisplaySlot(i, i, store.GetCommodity(i), store.GetCommodityStack(i));
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
			storeItemInfo.Display(CommodityName, true, !sellBuyButton.Pressed, alreadyHave);
		}
		private void OnBuySellToggled(bool buttonPressed)
		{
			PlaySound(NameDB.UI.CLICK1);
			sellBuyButton.Text = buttonPressed ? "Buy" : "Sell";
			if (player != null && merchant != null)
			{
				if (buttonPressed)
				{
					DisplayItemsPlayer();
				}
				else
				{
					DisplayItemsNpc(Globals.contentDB.GetData(merchant.Name).merchandise);
				}
			}
		}
	}
}