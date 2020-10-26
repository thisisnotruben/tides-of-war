using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class ItemDB
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

		private static Dictionary<string, ItemData> itemData = new Dictionary<string, ItemData>();
		private const string DB_PATH = "res://data/item.json";

		static ItemDB() { LoadItemData(); }
		public static void Init() { }
		private static void LoadItemData()
		{
			File file = new File();
			file.Open(DB_PATH, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string itemName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[itemName];

				// add to cache
				itemData.Add(itemName, new ItemData(
					type: (ItemType)Enum.Parse(typeof(ItemType), (string)dict[nameof(ItemData.type)]),
					icon: IconDB.GetIcon((int)((Single)dict[nameof(ItemData.icon)])),
					level: (int)((Single)dict[nameof(ItemData.level)]),
					goldCost: (int)((Single)dict[nameof(ItemData.goldCost)]),
					blurb: (string)dict[nameof(ItemData.blurb)],
					subType: (string)dict[nameof(ItemData.subType)],
					material: (string)dict[nameof(ItemData.material)],
					stackSize: (int)((Single)dict[nameof(ItemData.stackSize)]),
					coolDown: (int)((Single)dict[nameof(ItemData.coolDown)])
				));
			}
		}
		public static bool HasItem(string nameCheck) { return itemData.ContainsKey(nameCheck); }
		public static ItemData GetItemData(string worldName) { return itemData[worldName]; }
	}
}