using Godot;
namespace Game.Database
{
	public static class PickableDB
	{
		public static int GetStackSize(string worldName)
		{
			return Globals.spellDB.HasData(worldName)
				? Globals.spellDB.GetData(worldName).stackSize
				: Globals.itemDB.GetData(worldName).stackSize;
		}
		public static Texture GetIcon(string worldName)
		{
			return Globals.spellDB.HasData(worldName)
				? Globals.spellDB.GetData(worldName).icon
				: Globals.itemDB.GetData(worldName).icon;
		}
		public static int GetGoldCost(string worldName)
		{
			return Globals.spellDB.HasData(worldName)
				? Globals.spellDB.GetData(worldName).goldCost
				: Globals.itemDB.GetData(worldName).goldCost;
		}
	}
}
