using Godot;
using System;
using Game.Utils;
using Game.Loot;
using Game.Actor;
namespace Game.Ui
{
    public class SpellBookNode : Control
    {
        private Player _player;
        public Player player
        {
            set
            {
                _player = value;
                itemInfoNodeSpell.player = player;
            }
            get
            {
                return _player;
            }
        }        private Speaker _speaker;
        public Speaker speaker
        {
            set
            {
                _speaker = value;
                itemInfoNodeSpell.speaker = speaker;
            }
            get
            {
                return _speaker;
            }
        }
        private ItemList itemList = null;
        private ItemInfoNodeSpell itemInfoNodeSpell = null;

        public override void _Ready()
        {
            itemList = GetNode<ItemList>("s/v/c/spell_list");
            itemInfoNodeSpell.Connect("hide", this, nameof(Show));
            itemInfoNodeSpell = GetNode<ItemInfoNodeSpell>("item_info_node");
            itemInfoNodeSpell.itemList = itemList;
        }
        public void _OnBagIndexSelected(int index)
        {
            Globals.PlaySound("spell_select", this, speaker);
            Pickable pickable = itemList.GetItemMetaData(index);
            Hide();
            itemInfoNodeSpell.Display(pickable, true);
            itemInfoNodeSpell.Show();
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}
