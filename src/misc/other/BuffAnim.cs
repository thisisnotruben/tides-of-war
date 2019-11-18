using Game.Misc.Loot;
using Godot;
namespace Game.Misc.Other
{
    public class BuffAnim : Particles2D
    {
        private bool elixir = true;
        private Item item;
        public override void _Ready()
        {
            Color color = new Color("#ffffff");
            switch (item.GetPickableSubType())
            {
                case Item.WorldTypes.HEALING:
                    color = new Color("ff0000");
                    elixir = false;
                    break;
                case Item.WorldTypes.MANA:
                    color = new Color("0074a1");
                    elixir = false;
                    break;
                case Item.WorldTypes.STAMINA:
                    color = new Color("a52a2a");
                    break;
                case Item.WorldTypes.INTELLECT:
                    color = new Color("800080");
                    break;
                case Item.WorldTypes.AGILITY:
                    color = new Color("00ff00");
                    break;
                case Item.WorldTypes.STRENGTH:
                    color = new Color("ffff00");
                    break;
                case Item.WorldTypes.DEFENSE:
                    color = new Color("808080");
                    break;
            }
            Particles2D effect = GetNode<Particles2D>("buff_after_effect");
            effect.SetModulate(color);
            SetModulate(color);
            SetEmitting(true);
        }
        public void _OnTimerTimeout()
        {
            QueueFree();
        }
        public void SetItem(Item item)
        {
            this.item = item;
            SetName(item.GetInstanceId().ToString());
        }
    }
}