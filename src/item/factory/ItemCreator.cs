using Game.Actor;
namespace Game.ItemPoto
{
	public class ItemCreator : CommodityCreator
	{
		protected override Commodity CreateCommodity(Character character, string worldName)
		{
			return new Item(character, worldName);
		}
	}
}