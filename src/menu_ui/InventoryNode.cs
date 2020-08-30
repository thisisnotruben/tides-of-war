using Godot;
using Game.Loot;
using Game.ItemPoto;
using Game.Database;
namespace Game.Ui
{
	public class InventoryNode : GameMenu
	{
		private ItemList inventory;
		private ItemInfoNodeInventory itemInfoNodeInventory;

		public override void _Ready()
		{
			inventory = GetNode<ItemList>("s/v/c/inventory");
			itemInfoNodeInventory = GetNode<ItemInfoNodeInventory>("item_info");
			itemInfoNodeInventory.itemList = inventory;
			itemInfoNodeInventory.Connect("hide", this, nameof(_OnInventoryNodeHide));
			itemInfoNodeInventory.Connect(
				nameof(ItemInfoNodeInventory.ItemEquipped), this, nameof(_OnItemEquipped));
		}
		public void _OnInventoryNodeHide()
		{
			// TODO: new sound please
			// Globals.PlaySound("merchant_close", this, speaker);
			itemInfoNodeInventory.Hide();
			GetNode<Control>("s").Show();
		}
		public void _OnItemEquipped(Commodity item, bool on)
		{
			ItemDB.ItemNode itemNode = ItemDB.GetItemData(item.worldName);

			GetNode<TextureRect>(
				(itemNode.type == ItemDB.ItemType.ARMOR)
				? "s/v/slots/armor/m/icon"
				: "s/v/slots/weapon/m/icon")
					.Texture = (on) ? itemNode.icon : null;
		}
		public void _OnInventoryIndexSelected(int index)
		{
			Globals.PlaySound("inventory_open", this, speaker);
			string pickableWorldName = inventory.GetItemMetaData(index);
			GetNode<Control>("s").Hide();
			itemInfoNodeInventory.Display(pickableWorldName, true);
		}
		public void _OnEquippedSlotMoved(string nodePath, bool down)
		{
			float scale = (down) ? 0.8f : 1.0f;
			GetNode<Control>(nodePath).RectScale = new Vector2(scale, scale);
		}
		public void _OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player.weapon != null) || (!weapon && player.vest != null))
			{
				itemInfoNodeInventory.Display((weapon) ? player.weapon.worldName : player.vest.worldName, false);
			}
		}
	}
}
