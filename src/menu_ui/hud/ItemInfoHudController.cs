using Godot;
using Game.Database;
namespace Game.Ui
{
	public class ItemInfoHudController : ItemInfoInventoryController
	{
		public CanvasItem tabContainer, parent;
		public bool menuShown;

		public override void _Ready()
		{
			base._Ready();
			backBttn.Connect("pressed", this, nameof(OnClose));
			parent = GetParent<CanvasItem>();
			foreach (Node node in GetTree().GetNodesInGroup(Globals.HUD_SHORTCUT_GROUP))
			{
				(node as SlotController)?.Connect(nameof(SlotController.OnSlotPressed), this, nameof(OnHudSlotPressed));
			}
		}
		private void OnHudSlotPressed(string itemName)
		{
			if (menuShown || !Globals.itemDB.HasData(itemName))
			{
				return;
			}
			else if (itemName.Equals(commodityWorldName))
			{
				OnClose();
				return;
			}

			commodityWorldName = itemName;
			selectedSlotIdx = GetSlotIndexFromHud(itemName);

			switch (Globals.itemDB.GetData(itemName).type)
			{
				case ItemDB.ItemType.FOOD:
				case ItemDB.ItemType.POTION:
					OnUsePressed();
					tabContainer.SelfModulate = Color.ColorN("white", 0.0f);
					tabContainer.Visible = parent.Visible = Visible = popup.IsVisibleInTree();
					break;

				default:
					Display(itemName, false);
					tabContainer.SelfModulate = Color.ColorN("white");
					tabContainer.Visible = parent.Visible = Visible = true;
					break;
			}
		}
		protected override void OnHide()
		{
			base.OnHide();
			commodityWorldName = string.Empty;
			tabContainer.Visible = parent.Visible = false;
		}
		private void OnClose()
		{
			PlaySound(NameDB.UI.CLICK3);
			Hide();
		}
		public void OnGameMenuVisibilityChanged(bool shown) { menuShown = shown; }
	}
}