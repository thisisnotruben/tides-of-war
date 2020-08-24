using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class ItemDB
	{
		public struct ItemNode
		{
			public string type;
			public Texture icon;
			public int level;
			public int goldCost;
			public string blurb;
			public string subType;
			public string material;
			public int stackSize;
			public int coolDown;
		}
		private static Dictionary<string, ItemNode> itemData = new Dictionary<string, ItemNode>();
		private const string DB_PATH = "res://data/item.json";

		static ItemDB() { LoadItemData(); }
		private static void LoadItemData()
		{
			File file = new File();
			file.Open(DB_PATH, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();
			Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string itemName in rawDict.Keys)
			{
				Godot.Collections.Dictionary itemDict = (Godot.Collections.Dictionary)rawDict[itemName];
				ItemNode itemNode;
				itemNode.type = (string)itemDict[nameof(ItemNode.type)];
				itemNode.icon = IconDB.GetIcon((int)((Single)itemDict[nameof(ItemNode.icon)]));
				itemNode.level = (int)((Single)itemDict[nameof(ItemNode.level)]);
				itemNode.goldCost = (int)((Single)itemDict[nameof(ItemNode.goldCost)]);
				itemNode.blurb = (string)itemDict[nameof(ItemNode.blurb)];
				itemNode.subType = (string)itemDict[nameof(ItemNode.subType)];
				itemNode.material = (string)itemDict[nameof(ItemNode.material)];
				itemNode.stackSize = (int)((Single)itemDict[nameof(ItemNode.stackSize)]);
				itemNode.coolDown = 1; // TODO
				itemData.Add(itemName, itemNode);
			}
		}
		public static ItemNode GetItemData(string worldName) { return itemData[worldName]; }
		public static string GetItemMaterial(string worldName) { return itemData[worldName].material; }
		public static bool HasItem(string nameCheck) { return itemData.ContainsKey(nameCheck); }
	}
}