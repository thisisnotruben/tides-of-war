using System;
using Godot;
namespace Game.Database
{
	public class ItemDB : AbstractDB<ItemDB.ItemData>
	{
		public enum ItemType { NONE, WEAPON, ARMOR, POTION, FOOD, MISC, QUEST, EVENT }
		public class ItemData
		{
			public readonly ItemType type;
			public readonly Texture icon;
			public readonly int level, goldCost, stackSize, coolDown;
			public readonly float dropRate;
			public readonly string blurb, subType, material;

			public ItemData(ItemType type, Texture icon, int level, int goldCost, int stackSize,
			int coolDown, float dropRate, string blurb, string subType, string material)
			{
				this.type = type;
				this.icon = icon;
				this.level = level;
				this.goldCost = goldCost;
				this.stackSize = stackSize;
				this.coolDown = coolDown;
				this.dropRate = dropRate;
				this.blurb = blurb;
				this.subType = subType;
				this.material = material;
			}
		}

		public ItemDB(string path) : base(path) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string itemName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[itemName];

				data.Add(itemName, new ItemData(
					type: (ItemType)Enum.Parse(typeof(ItemType), dict[nameof(ItemData.type)].ToString()),
					icon: IconDB.GetIcon(dict[nameof(ItemData.icon)].ToString().ToInt()),
					level: dict[nameof(ItemData.level)].ToString().ToInt(),
					goldCost: dict[nameof(ItemData.goldCost)].ToString().ToInt(),
					blurb: dict[nameof(ItemData.blurb)].ToString(),
					subType: dict[nameof(ItemData.subType)].ToString(),
					material: dict[nameof(ItemData.material)].ToString(),
					stackSize: dict[nameof(ItemData.stackSize)].ToString().ToInt(),
					coolDown: dict[nameof(ItemData.coolDown)].ToString().ToInt(),
					dropRate: dict[nameof(ItemData.dropRate)].ToString().ToFloat()
				));
			}
		}
	}
}