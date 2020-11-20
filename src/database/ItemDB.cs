using System;
using Godot;
namespace Game.Database
{
	public class ItemDB : AbstractDB<ItemDB.ItemData>
	{
		public enum ItemType { WEAPON, ARMOR, POTION, FOOD, MISC }
		public class ItemData
		{
			public readonly ItemType type;
			public readonly Texture icon;
			public readonly int level, goldCost, stackSize, coolDown;
			public readonly string blurb, subType, material;

			public ItemData(ItemType type, Texture icon, int level, int goldCost, int stackSize,
			int coolDown, string blurb, string subType, string material)
			{
				this.type = type;
				this.icon = icon;
				this.level = level;
				this.goldCost = goldCost;
				this.stackSize = stackSize;
				this.coolDown = coolDown;
				this.blurb = blurb;
				this.subType = subType;
				this.material = material;
			}
		}

		public static readonly ItemDB Instance = new ItemDB();

		public ItemDB() : base(PathManager.item) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string itemName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[itemName];

				data.Add(itemName, new ItemData(
					type: (ItemType)Enum.Parse(typeof(ItemType), (string)dict[nameof(ItemData.type)]),
					icon: IconDB.GetIcon((int)(Single)dict[nameof(ItemData.icon)]),
					level: (int)(Single)dict[nameof(ItemData.level)],
					goldCost: (int)(Single)dict[nameof(ItemData.goldCost)],
					blurb: (string)dict[nameof(ItemData.blurb)],
					subType: (string)dict[nameof(ItemData.subType)],
					material: (string)dict[nameof(ItemData.material)],
					stackSize: (int)(Single)dict[nameof(ItemData.stackSize)],
					coolDown: (int)(Single)dict[nameof(ItemData.coolDown)]
				));
			}
		}
	}
}