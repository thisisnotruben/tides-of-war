using Godot;
namespace Game.Database
{
	public static class PickableDB
	{
		public static int GetStackSize(string worldName)
		{
			return (SpellDB.HasSpell(worldName))
				? SpellDB.GetSpellData(worldName).stackSize
				: ItemDB.GetItemData(worldName).stackSize;
		}
		public static Texture GetIcon(string worldName)
		{
			return (SpellDB.HasSpell(worldName))
				? SpellDB.GetSpellData(worldName).icon
				: ItemDB.GetItemData(worldName).icon;
		}
		public static int GetGoldCost(string worldName)
		{
			return (SpellDB.HasSpell(worldName))
				? SpellDB.GetSpellData(worldName).goldCost
				: ItemDB.GetItemData(worldName).goldCost;
		}
	}
}
