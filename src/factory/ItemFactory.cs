using Game.Actor;
using Game.GameItem;
namespace Game.Factory
{
	public class ItemFactory : CommodityFactory
	{
		protected override Commodity CreateCommodity(Character character, string worldName)
		{
			return new Item().Init(character, worldName);
		}
	}
}