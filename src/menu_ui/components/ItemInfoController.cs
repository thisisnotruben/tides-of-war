using System.Linq;
using Game.Database;
using Game.GameItem;
using Game.Sound;
using Godot;
namespace Game.Ui
{
	public class ItemInfoController : GameMenu
	{
		public InventoryModel itemList;
		public int selectedSlotIdx;
		protected PopupController popupController;
		protected string pickableWorldName;

		public override void _Ready()
		{
			// connect popup events
			popupController = GetNode<PopupController>("popup");
			popupController.GetNode<BaseButton>("m/add_to_slot/clear_slot")
				.Connect("pressed", this, nameof(_OnClearSlotPressed));
			popupController.GetNode<BaseButton>("m/add_to_slot/back")
				.Connect("pressed", this, nameof(_OnAddToHudBack));
			popupController.Connect("hide", this, nameof(_OnItemInfoNodeHide));
			for (int i = 1; i <= 4; i++)
			{
				popupController.GetNode($"m/add_to_slot/slot_{i}").Connect("pressed",
					this, nameof(_OnAddToHudConfirm), new Godot.Collections.Array() { i });
			}
		}
		public virtual void Display(string pickableWorldName, bool allowMove)
		{
			this.pickableWorldName = pickableWorldName;

			// set arrow move constraints
			TextureButton left = GetNode<TextureButton>("s/h/left");
			TextureButton right = GetNode<TextureButton>("s/h/right");

			// itemList != null due to player traversing through shortcuts
			if (allowMove && itemList != null)
			{
				// disable left button if at first item
				left.Disabled = selectedSlotIdx == 0;
				// disable right button if at last time
				right.Disabled = selectedSlotIdx == itemList.count - 1;

				left.Show();
				right.Show();
			}
			else
			{
				left.Hide();
				right.Hide();
			}

			// decide which buttons to show
			string[] showBttns = { };
			if (!player.dead && itemList != null && ItemDB.HasItem(pickableWorldName))
			{
				showBttns = new string[] { "drop", "" };
				ItemDB.ItemType itemType = ItemDB.GetItemData(pickableWorldName).type;
				switch (itemType)
				{
					case ItemDB.ItemType.FOOD:
					case ItemDB.ItemType.POTION:
						if (!Commodity.IsCoolingDown(player.GetPath(), pickableWorldName))
						{
							showBttns[1] = "use";
							GetNode<Label>("s/h/buttons/use/label").Text =
								(itemType == ItemDB.ItemType.FOOD) ? "Eat" : "Drink";
						}
						break;
					case ItemDB.ItemType.WEAPON:
					case ItemDB.ItemType.ARMOR:
						showBttns[1] = "equip";
						break;
				}
			}

			// set presentation
			HideExcept(showBttns);
			GetNode<Label>("s/v/header").Text = pickableWorldName;
			GetNode<TextureRect>("s/v/c/v/add_to_hud/m/icon").Texture = PickableDB.GetIcon(pickableWorldName);
			GetNode<RichTextLabel>("s/v/c/v/m/info_text").BbcodeText = Commodity.GetDescription(pickableWorldName);

			Show();
		}
		protected void HideExcept(params string[] nodesToShow)
		{
			foreach (Control node in GetNode("s/h/buttons").GetChildren())
			{
				node.Visible = nodesToShow.Contains(node.Name) || node.Name.Equals("back");
			}
		}
		protected void RouteConnections(string toMethod)
		{
			BaseButton yesBttn = popupController.GetNode<BaseButton>("m/yes_no/yes");
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
			GetNode<Control>("s").Show();
		}
		public void _OnSlotMoved(string nodePath, bool down)
		{
			GetNode<Control>(nodePath).RectScale =
				(down) ? new Vector2(0.8f, 0.8f) : new Vector2(1.0f, 1.0f);
		}
		public void _OnAddToHudPressed()
		{
			// TODO
			// Globals.PlaySound("click2", this, speaker);
			// GetNode<Control>("s").Hide();
			// int count = 1;
			// popupController.GetNode<Control>("m/add_to_slot/clear_slot").Hide();
			// foreach (SlotController itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			// {
			// 	Tween tween = itemSlot.GetNode<Tween>("tween");
			// 	ColorRect colorRect = itemSlot.GetNode<ColorRect>("m/icon/overlay");
			// 	Label label = itemSlot.GetNode<Label>("m/label");
			// 	if (tween.IsActive())
			// 	{
			// 		tween.SetActive(false);
			// 		colorRect.RectScale = new Vector2(1.0f, 1.0f);
			// 	}
			// 	colorRect.Color = new Color(1.0f, 1.0f, 0.0f, 0.75f);
			// 	label.Text = count.ToString();
			// 	label.Show();
			// 	if (!itemSlot.GetItem().Empty() && itemSlot.GetItem().Equals(pickableWorldName))
			// 	{
			// 		popupController.GetNode<Control>("m/add_to_slot/clear_slot").Show();
			// 	}
			// 	count++;
			// }
			// popupController.GetNode<Control>("m/add_to_slot").Show();
			// popupController.Show();
		}
		public void _OnAddToHudConfirm(int index)
		{
			// TODO
			// int amounttt = -1;
			// ItemSlot buttonFrom = null;
			// ItemSlot buttonTo = null;
			// Hide();
			// Globals.PlaySound("click1", this, speaker);
			// foreach (ItemSlot shortcut in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			// {
			// 	Tween itemSlotTween = shortcut.GetNode<Tween>("tween");
			// 	shortcut.GetNode<ColorRect>("m/icon/overlay").Color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
			// 	shortcut.GetNode<Control>("m/label").Hide();
			// 	itemSlotTween.SetActive(true);
			// 	itemSlotTween.ResumeAll();
			// 	if (!shortcut.GetItem().Empty() && shortcut.GetItem().Equals(pickableWorldName))
			// 	{
			// 		amounttt = shortcut.GetItemStack().Count;
			// 		shortcut.SetItem(null, false, true, false);
			// 	}
			// 	if (shortcut.Name.Equals(index.ToString()))
			// 	{
			// 		buttonTo = shortcut;
			// 		if (shortcut.GetItem() != null)
			// 		{
			// 			shortcut.SetItem(null, false, true, false);
			// 		}
			// 		string weaponWorldName = (player.weapon == null) ? "" : player.weapon.worldName;
			// 		string armorWorldName = (player.vest == null) ? "" : player.vest.worldName;
			// 		if (weaponWorldName.Equals(pickableWorldName))
			// 		{
			// 			shortcut.SetItem(weaponWorldName, false, false, false);
			// 		}
			// 		else if (armorWorldName.Equals(pickableWorldName))
			// 		{
			// 			shortcut.SetItem(armorWorldName, false, false, false);
			// 		}
			// 		else
			// 		{
			// 			ItemSlot itemSlot = itemList.GetItemSlot(pickableWorldName);
			// 			List<string> pickableStack = itemSlot.GetItemStack();
			// 			buttonFrom = itemSlot;
			// 			if (amounttt == -1)
			// 			{
			// 				amounttt = pickableStack.Count;
			// 			}
			// 			for (int i = 0; i < amounttt; i++)
			// 			{
			// 				shortcut.SetItem(pickableStack[i]);
			// 			}
			// 		}
			// 	}
			// }
			// if (buttonFrom != null && buttonTo != null)
			// {
			// 	foreach (Godot.Collections.Dictionary link in buttonFrom.GetSignalConnectionList(nameof(ItemSlot.SyncSlot)))
			// 	{
			// 		buttonFrom.Disconnect(nameof(ItemSlot.SyncSlot), (Godot.Object)link["target"], nameof(ItemSlot._OnSyncShortcut));
			// 	}
			// 	buttonFrom.Connect(nameof(ItemSlot.SyncSlot), buttonTo, nameof(ItemSlot._OnSyncShortcut));
			// 	if (buttonFrom.IsCoolingDown())
			// 	{
			// 		buttonTo.CoolDown(buttonFrom.GetItem(), buttonFrom.GetCoolDownInitialTime(), buttonFrom.GetCoolDownTimeLeft());
			// 	}
			// }
		}
		public void _OnAddToHudBack()
		{
			foreach (SlotController itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				Tween tween = itemSlot.GetNode<Tween>("tween");
				tween.SetActive(true);
				tween.ResumeAll();
				itemSlot.GetNode<ColorRect>("m/icon/overlay").Color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
				itemSlot.GetNode<Control>("m/label").Hide();
			}
		}
		public void _OnClearSlotPressed()
		{
			// TODO
			// foreach (SlotController itemSlot in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			// {
			// 	if (itemSlot.GetItem() != null && itemSlot.GetItem().Equals(pickableWorldName))
			// 	{
			// 		itemSlot.SetItem(null, false, true, false);
			// 	}
			// }
			popupController.Hide();
		}
		public void _OnInfoTextDraw()
		{
			Vector2 correctSize = GetNode<Control>("s/v/c/v/m").GetRect().Size;
			GetNode<RichTextLabel>("s/v/c/v/m/info_text").RectMinSize = correctSize;
		}
		public virtual void _OnMovePressed(int by)
		{
			SoundPlayer.INSTANCE.PlaySound("click2");
			// update selection and display
			selectedSlotIdx += by;
			Display(itemList.GetCommodity(selectedSlotIdx), true);
		}
	}
}