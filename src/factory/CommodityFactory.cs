using Game.Actor;
using Game.GameItem;
namespace Game.Factory
{
	public abstract class CommodityFactory
	{
		public Commodity MakeCommodity(Character character, string worldName) { return CreateCommodity(character, worldName); }
		protected abstract Commodity CreateCommodity(Character character, string worldName);
	}
}