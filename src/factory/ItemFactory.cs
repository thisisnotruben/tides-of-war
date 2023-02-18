using Game.Actor;
using Game.GameItem;
namespace Game.Factory
{
	public class ItemFactory : Factory<Item>
	{
		protected override Item Create(Character character, string worldName)
		{
			Item item = new Item();
			item.Init(character, worldName);
			return item;
		}
	}
}