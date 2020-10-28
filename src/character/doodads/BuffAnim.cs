using Game.GameItem;
using Godot;
namespace Game.Actor.Doodads
{
	public class BuffAnim : Particles2D
	{
		private bool elixir = true;
		private Commodity _item;
		public Commodity item
		{
			get { return _item; }
			set
			{
				_item = value;
				Name = item.GetInstanceId().ToString();
			}
		}
		public override void _Ready()
		{
			Color color = new Color("#ffffff");
			// TODO
			// switch (ItemDB.GetItemData(item.worldName).type)
			// {
			// 	case Item.WorldTypes.HEALING:
			// 		color = new Color("ff0000");
			// 		elixir = false;
			// 		break;
			// 	case Item.WorldTypes.MANA:
			// 		color = new Color("0074a1");
			// 		elixir = false;
			// 		break;
			// 	case Item.WorldTypes.STAMINA:
			// 		color = new Color("a52a2a");
			// 		break;
			// 	case Item.WorldTypes.INTELLECT:
			// 		color = new Color("800080");
			// 		break;
			// 	case Item.WorldTypes.AGILITY:
			// 		color = new Color("00ff00");
			// 		break;
			// 	case Item.WorldTypes.STRENGTH:
			// 		color = new Color("ffff00");
			// 		break;
			// 	case Item.WorldTypes.DEFENSE:
			// 		color = new Color("808080");
			// 		break;
			// }
			Particles2D effect = GetNode<Particles2D>("buff_after_effect");
			effect.Modulate = color;
			Modulate = color;
			Emitting = true;
		}
		public void _OnTimerTimeout() { QueueFree(); }
	}
}