using System.Linq;
using Game.Database;
using Game.GameItem;
using Godot;
namespace Game.Ui
{
	public class ItemInfoController : GameMenu
	{
		public InventoryModel inventoryModel;
		public SlotGridController slotGridController;
		public int selectedSlotIdx = -1;
		public PopupController popup;
		protected Label header;
		protected RichTextLabel richTextLabel;
		public string commodityWorldName;
		protected Control mainContent, buttonContainer;
		protected ItemInfoSlotController iconView;
		public Button castBttn, useBttn, buyBttn, sellBttn, equipBttn,
			unequipBttn, dropBttn, backBttn, leftbttn, rightBttn;

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);

			header = GetNode<Label>("s/vBoxContainer/header");
			iconView = GetNode<ItemInfoSlotController>("s/vBoxContainer/icon");
			richTextLabel = GetNode<RichTextLabel>("s/vBoxContainer/richTextLabel");

			buttonContainer = GetNode<Control>("s/hBoxContainer/gridContainer");
			buttonContainer.Connect("sort_children", this, nameof(OnButtonContainerSort));
			castBttn = buttonContainer.GetNode<Button>("cast");
			useBttn = buttonContainer.GetNode<Button>("use");
			buyBttn = buttonContainer.GetNode<Button>("buy");
			sellBttn = buttonContainer.GetNode<Button>("sell");
			equipBttn = buttonContainer.GetNode<Button>("equip");
			unequipBttn = buttonContainer.GetNode<Button>("unequip");
			dropBttn = buttonContainer.GetNode<Button>("drop");
			backBttn = buttonContainer.GetNode<Button>("back");
			leftbttn = GetNode<Button>("s/hBoxContainer/left");
			rightBttn = GetNode<Button>("s/hBoxContainer/right");

			// connect popup events
			popup = GetChild<PopupController>(1);
			popup.Connect("hide", this, nameof(OnPopupHide));
		}
		public virtual void Display(string commodityWorldName, bool allowMove)
		{
			this.commodityWorldName = commodityWorldName;

			// inventoryModel != null due to player traversing through shortcuts
			if (allowMove && inventoryModel != null)
			{
				leftbttn.Disabled = !slotGridController.IsSlotUsed(
					slotGridController.GetNextSlot(selectedSlotIdx, false));
				rightBttn.Disabled = !slotGridController.IsSlotUsed(
					slotGridController.GetNextSlot(selectedSlotIdx, true));
				leftbttn.Visible = rightBttn.Visible = true;
			}
			else
			{
				leftbttn.Visible = rightBttn.Visible = false;
			}

			// decide which buttons to show
			Control[] showBttns = { };
			if (!player.dead && inventoryModel != null && Globals.itemDB.HasData(commodityWorldName))
			{
				showBttns = new Control[] { dropBttn, null };
				ItemDB.ItemType itemType = Globals.itemDB.GetData(commodityWorldName).type;
				switch (itemType)
				{
					case ItemDB.ItemType.FOOD:
					case ItemDB.ItemType.POTION:
						if (!Globals.cooldownMaster.IsCoolingDown(player.GetPath(), commodityWorldName))
						{
							showBttns[1] = useBttn;
							useBttn.Text = itemType == ItemDB.ItemType.FOOD ? "Eat" : "Drink";
						}
						break;
					case ItemDB.ItemType.WEAPON:
					case ItemDB.ItemType.ARMOR:
						showBttns[1] = equipBttn;
						break;
				}
			}

			// set presentation
			HideExcept(showBttns);
			header.Text = commodityWorldName;
			iconView.Display(commodityWorldName);
			richTextLabel.BbcodeText = Commodity.GetDescription(commodityWorldName);
			Show();
		}
		protected void HideExcept(params Control[] nodesToShow)
		{
			foreach (Control node in GetNode("s/hBoxContainer/gridContainer").GetChildren())
			{
				node.Visible = nodesToShow.Contains(node) || node == backBttn;
			}
		}
		protected virtual void OnHide()
		{
			popup.Hide();
			mainContent.Show();
			selectedSlotIdx = -1;
		}
		protected virtual void OnPopupHide() { mainContent.Show(); }
		public virtual void OnMovePressed(bool forward)
		{
			PlaySound(NameDB.UI.CLICK2);
			// update selection and display
			selectedSlotIdx = slotGridController.GetNextSlot(selectedSlotIdx, forward);
			Display(inventoryModel.GetCommodity(
				slotGridController.GetSlotToModelIndex(selectedSlotIdx))
				, true);
		}
		private void OnButtonContainerSort()
		{
			int visible = 0;
			foreach (Control button in buttonContainer.GetChildren())
			{
				if (button.Visible)
				{
					visible++;
				}
			}
			if (visible % 2 == 1)
			{
				Control lastButton = buttonContainer.GetChild<Control>(buttonContainer.GetChildCount() - 1);
				lastButton.RectSize = new Vector2(buttonContainer.RectSize.x, lastButton.RectSize.y);
			}
		}
	}
}