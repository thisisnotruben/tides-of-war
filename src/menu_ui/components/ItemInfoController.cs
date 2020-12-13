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
		protected string commodityWorldName;
		protected Control mainContent;
		protected TextureButton leftbttn, rightBttn;
		protected TextureRect commodityIcon;
		public Button castBttn, useBttn, buyBttn, sellBttn, equipBttn, unequipBttn, dropBttn, backBttn;
		public TextureButton addToHudBttn;

		public override void _Ready()
		{
			// connect popup events
			popupController = GetNode<PopupController>("popup");
			popupController.clearSlot.Connect("pressed", this, nameof(_OnClearSlotPressed));
			popupController.Connect("hide", this, nameof(_OnAddToHudBack));
			popupController.Connect("hide", this, nameof(_OnItemInfoNodeHide));
			popupController.addToHudSlotBttns.ForEach(b => b.Connect("pressed",
				this, nameof(_OnAddToHudConfirm), new Godot.Collections.Array() { b.GetIndex() + 1 })
			);

			mainContent = GetNode<Control>("s");

			addToHudBttn = GetNode<TextureButton>("s/v/c/v/add_to_hud");
			commodityIcon = addToHudBttn.GetNode<TextureRect>("m/icon");

			addToHudBttn.Connect("pressed", this, nameof(OnAddToHudPressed));
			addToHudBttn.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { addToHudBttn, true });
			addToHudBttn.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { addToHudBttn, false });

			castBttn = GetNode<Button>("s/h/buttons/cast");
			useBttn = GetNode<Button>("s/h/buttons/use");
			buyBttn = GetNode<Button>("s/h/buttons/buy");
			sellBttn = GetNode<Button>("s/h/buttons/sell");
			equipBttn = GetNode<Button>("s/h/buttons/equip");
			unequipBttn = GetNode<Button>("s/h/buttons/unequip");
			dropBttn = GetNode<Button>("s/h/buttons/drop");
			backBttn = GetNode<Button>("s/h/buttons/back");

			leftbttn = GetNode<TextureButton>("s/h/left");
			rightBttn = GetNode<TextureButton>("s/h/right");
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
			GetNode<Label>("s/v/header").Text = commodityWorldName;
			commodityIcon.Texture = PickableDB.GetIcon(commodityWorldName);
			GetNode<RichTextLabel>("s/v/c/v/m/info_text").BbcodeText = Commodity.GetDescription(commodityWorldName);
			Show();
		}
		protected void HideExcept(params Control[] nodesToShow)
		{
			foreach (Control node in GetNode("s/h/buttons").GetChildren())
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
		public void _OnItemInfoNodeHide()
		{
			popupController.Hide();
			mainContent.Show();
		}
		public void OnAddToHudPressed()
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
		public void _OnAddToHudConfirm(int index)
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
		public void _OnAddToHudBack()
		{
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				(node as HudSlotController)?.RevertAddToHudDisplay();
			}
		}
		public void _OnClearSlotPressed()
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
		public void _OnInfoTextDraw()
		{
			Vector2 correctSize = GetNode<Control>("s/v/c/v/m").GetRect().Size;
			GetNode<RichTextLabel>("s/v/c/v/m/info_text").RectMinSize = correctSize;
		}
		public virtual void _OnMovePressed(int by)
		{
			PlaySound(NameDB.UI.CLICK2);
			// update selection and display
			selectedSlotIdx += by;
			Display(itemList.GetCommodity(selectedSlotIdx), true);
		}
	}
}