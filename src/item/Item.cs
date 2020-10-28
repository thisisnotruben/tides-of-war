using Game.Actor;
namespace Game.GameItem
{
	public class Item : Commodity
	{
		public Item(Character character, string worldName) : base(character, worldName) { }
	}
}