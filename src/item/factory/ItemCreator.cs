using Game.Actor;
namespace Game.ItemPoto
{
	public class ItemCreator : CommodityCreator
	{
		private protected override Commodity CreateCommodity(Character character, string worldName)
		{
			return new Item(character, worldName);
		}
	}
}