using Game.Actor;
namespace Game.ItemPoto
{
	public abstract class CommodityCreator
	{
		public Commodity MakeCommodity(Character character, string worldName) { return CreateCommodity(character, worldName); }
		private protected abstract Commodity CreateCommodity(Character character, string worldName);
	}
}