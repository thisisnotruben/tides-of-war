using Godot;
using Game.Actor;
using Game.Loot;
using Game.Utils;

namespace Game.Ui
{
    public class InventoryNode : Control
    {
        private Player _player;
        public Player player
        {
            set
            {
                _player = value;
                itemInfoNodeInventory.player = value;
            }
            get
            {
                return _player;
            }
        }
        private Speaker _speaker;
        public Speaker speaker
        {
            set
            {
                _speaker = value;
                itemInfoNodeInventory.speaker = speaker;
            }
            get
            {
                return _speaker;
            }
        }
        private ItemList inventory = null;
        private ItemInfoNodeInventory itemInfoNodeInventory = null;

        public override void _Ready()
        {
            itemInfoNodeInventory = GetNode<ItemInfoNodeInventory>("item_info");
            itemInfoNodeInventory.itemList = inventory;
            itemInfoNodeInventory.Connect("hide", this, nameof(Show));
            inventory = GetNode<ItemList>("s/v/c/inventory");
        }
        public void _OnInventoryIndexSelected(int index)
        {
            Globals.PlaySound("inventory_open", this, speaker);
            Pickable pickable = inventory.GetItemMetaData(index);
            Hide();
            itemInfoNodeInventory.Display(pickable, true);
            itemInfoNodeInventory.Show();
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
                itemInfoNodeInventory.Display((weapon) ? player.weapon : player.vest, false);
            }
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}
