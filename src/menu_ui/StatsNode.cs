using Godot;
using System;
using Game.Actor;
using Game.Utils;

namespace Game.Ui
{
    public class StatsNode : Control
    {
        private Player _player;
        public Player player
        {
            set
            {
                _player = value;
                itemInfoNode.player = value;
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
                itemInfoNode.speaker = value;
            }
            get
            {
                return _speaker;
            }
        }
        private ItemInfoNode itemInfoNode;

        public override void _Ready()
        {
            itemInfoNode = GetNode<ItemInfoNode>("item_info");
            itemInfoNode.player = player;
            itemInfoNode.speaker = speaker;
            itemInfoNode.itemList = null;
            itemInfoNode.Connect("hide", this, nameof(Show));
            BaseButton addToHudBttn = itemInfoNode.GetNode<BaseButton>("s/v/c/v/add_to_hud");
            addToHudBttn.Disconnect("button_up", itemInfoNode, nameof(ItemInfoNode._OnSlotMoved));
            addToHudBttn.Disconnect("button_down", itemInfoNode, nameof(ItemInfoNode._OnSlotMoved));
            addToHudBttn.Disconnect("pressed", itemInfoNode, nameof(ItemInfoNode._OnAddToHudPressed));
        }
        public void _OnStatsNodeDraw()
        {
            GetNode<RichTextLabel>("s/v/c/label").BbcodeText =
                $"Name: {player.worldName}\nHealth: {player.hp} / {player.hpMax}\n" +
                $"Mana: {player.mana} / {player.manaMax}\nXP: {player.xp}\nLevel: {player.level}\n" +
                $"Gold: {player.gold.ToString("N0")}\nStamina: {player.stamina}\nIntellect: {player.intellect}\n" +
                $"Agility: {player.agility}\nArmor: {player.armor}\nDamage: {player.minDamage} - {player.maxDamage}\n" +
                $"Attack Speed: {player.weaponSpeed.ToString("0.00")}\nAttack Range: {player.weaponRange}";
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
                itemInfoNode.Display((weapon) ? player.weapon : player.vest, false);
            }
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}