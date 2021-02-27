using System.Linq;
using Game.Database;
using Game.GameItem;
using Godot;
namespace Game.Ui
{
	public class ItemInfoController : GameMenu
	{
		public InventoryModel itemList;
		public int selectedSlotIdx;
		public PopupController popupController;
		protected Label header;
		protected RichTextLabel richTextLabel;
		protected string commodityWorldName;
		protected Control mainContent, buttonContainer;
		protected TextureButton leftbttn, rightBttn;
		protected TextureRect commodityIcon;
		public Button castBttn, useBttn, buyBttn, sellBttn, equipBttn, unequipBttn, dropBttn, backBttn;
		public Button addToHudBttn;

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);

			header = GetNode<Label>("s/vBoxContainer/header");
			addToHudBttn = GetNode<Button>("s/vBoxContainer/add_to_hud");
			commodityIcon = addToHudBttn.GetNode<TextureRect>("m/icon");
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

			leftbttn = GetNode<TextureButton>("s/hBoxContainer/left");
			rightBttn = GetNode<TextureButton>("s/hBoxContainer/right");

			// connect popup events
			popupController = GetChild<PopupController>(1);
			popupController.clearSlot.Connect("pressed", this, nameof(OnClearSlotPressed));
			popupController.Connect("hide", this, nameof(OnAddToHudBack));
			popupController.Connect("hide", this, nameof(OnHide));
			popupController.addToHudSlotBttns.ForEach(b => b.Connect("pressed",
				this, nameof(OnAddToHudConfirm), new Godot.Collections.Array() { b.GetIndex() + 1 })
			);
		}
		public virtual void Display(string commodityWorldName, bool allowMove)
		{
			this.commodityWorldName = commodityWorldName;

			// itemList != null due to player traversing through shortcuts
			if (allowMove && itemList != null)
			{
				// disable left button if at first item
				leftbttn.Disabled = selectedSlotIdx == 0;
				// disable right button if at last time
				rightBttn.Disabled = selectedSlotIdx == itemList.count - 1;

				leftbttn.Show();
				rightBttn.Show();
			}
			else
			{
				leftbttn.Hide();
				rightBttn.Hide();
			}

			// decide which buttons to show
			Control[] showBttns = { };
			if (!player.dead && itemList != null && Globals.itemDB.HasData(commodityWorldName))
			{
				showBttns = new Control[] { dropBttn, null };
				ItemDB.ItemType itemType = Globals.itemDB.GetData(commodityWorldName).type;
				switch (itemType)
				{
					case ItemDB.ItemType.FOOD:
					case ItemDB.ItemType.POTION:
						if (!Commodity.IsCoolingDown(player.GetPath(), commodityWorldName))
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
			commodityIcon.Texture = PickableDB.GetIcon(commodityWorldName);
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
		protected void RouteConnections(string toMethod)
		{
			BaseButton yesBttn = popupController.yesBttn;
			string signal = "pressed";

			foreach (Godot.Collections.Dictionary connectionPacket in yesBttn.GetSignalConnectionList(signal))
			{
				yesBttn.Disconnect(signal, this, (string)connectionPacket["method"]);
			}
			yesBttn.Connect(signal, this, toMethod);
		}
		private void OnHide()
		{
			popupController.Hide();
			mainContent.Show();
		}
		private void OnAddToHudPressed()
		{
			PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			popupController.clearSlot.Hide();

			int count = 1;
			HudSlotController hudControlController;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				hudControlController = node as HudSlotController;
				if (hudControlController?.HasCommodity(commodityWorldName) ?? false)
				{
					popupController.clearSlot.Show();
				}
				hudControlController?.ShowAddToHudDisplay((count++).ToString());
			}
			popupController.addToSlotView.Show();
			popupController.Show();
		}
		private void OnAddToHudConfirm(int index)
		{
			PlaySound(NameDB.UI.CLICK1);
			Hide();

			HudSlotController hudSlotController;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				hudSlotController = node as HudSlotController;
				if (hudSlotController?.Name.Equals(index.ToString()) ?? false)
				{
					hudSlotController.Display(commodityWorldName, player.GetPath(), itemList);

					// TODO: connect pressed events to some form of action

					return;
				}
			}
		}
		private void OnAddToHudBack()
		{
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				(node as HudSlotController)?.RevertAddToHudDisplay();
			}
		}
		private void OnClearSlotPressed()
		{
			HudSlotController hudSlotController;
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				hudSlotController = node as HudSlotController;
				if (hudSlotController?.HasCommodity(commodityWorldName) ?? false)
				{
					hudSlotController.ClearDisplay();
					break;
				}
			}
			popupController.Hide();
		}
		public virtual void _OnMovePressed(int by)
		{
			PlaySound(NameDB.UI.CLICK2);
			// update selection and display
			selectedSlotIdx += by;
			Display(itemList.GetCommodity(selectedSlotIdx), true);
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