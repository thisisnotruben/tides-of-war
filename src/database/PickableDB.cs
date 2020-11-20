using Godot;
namespace Game.Database
{
	public static class PickableDB
	{
		public static int GetStackSize(string worldName)
		{
			return SpellDB.Instance.HasData(worldName)
				? SpellDB.Instance.GetData(worldName).stackSize
				: ItemDB.Instance.GetData(worldName).stackSize;
		}
		public static Texture GetIcon(string worldName)
		{
			return SpellDB.Instance.HasData(worldName)
				? SpellDB.Instance.GetData(worldName).icon
				: ItemDB.Instance.GetData(worldName).icon;
		}
		public static int GetGoldCost(string worldName)
		{
			return SpellDB.Instance.HasData(worldName)
				? SpellDB.Instance.GetData(worldName).goldCost
				: ItemDB.Instance.GetData(worldName).goldCost;
		}
	}
}
