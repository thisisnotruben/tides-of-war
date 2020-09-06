using Game.Actor;
using Game.Ability;
namespace Game.ItemPoto
{
	public class SpellCreator : CommodityCreator
	{
		private protected override Commodity CreateCommodity(Character character, string worldName)
		{
			return new SpellProto(character, worldName);
		}
	}
}