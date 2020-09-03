using System.Collections.Generic;
using System;
using Game.Actor.Stat;
using Godot;
namespace Game.Database
{
	public static class ItemDB
	{
		public enum ItemType { WEAPON, ARMOR, POTION, FOOD, MISC }
		public struct ItemNode
		{
			public ItemType type;
			public Texture icon;
			public int level;
			public int goldCost;
			public string blurb;
			public string subType;
			public string material;
			public int stackSize;
			public int coolDown;
			public Modifiers modifiers;
			public Use use;
		}
		public struct Modifiers
		{
			public int duration;
			public ModifierNode stamina;
			public ModifierNode intellect;
			public ModifierNode agility;
			public ModifierNode hpMax;
			public ModifierNode manaMax;
			public ModifierNode maxDamage;
			public ModifierNode minDamage;
			public ModifierNode regenTime;
			public ModifierNode armor;
			public ModifierNode weaponRange;
			public ModifierNode weaponSpeed;
		}
		public struct ModifierNode
		{
			public StatModifier.StatModType type;
			public float value;
		}
		public struct Use
		{
			public int hp;
			public int mana;
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
				itemNode.type = (ItemType)Enum.Parse(typeof(ItemType), (string)itemDict[nameof(ItemNode.type)]);
				itemNode.icon = IconDB.GetIcon((int)((Single)itemDict[nameof(ItemNode.icon)]));
				itemNode.level = (int)((Single)itemDict[nameof(ItemNode.level)]);
				itemNode.goldCost = (int)((Single)itemDict[nameof(ItemNode.goldCost)]);
				itemNode.blurb = (string)itemDict[nameof(ItemNode.blurb)];
				itemNode.subType = (string)itemDict[nameof(ItemNode.subType)];
				itemNode.material = (string)itemDict[nameof(ItemNode.material)];
				itemNode.stackSize = (int)((Single)itemDict[nameof(ItemNode.stackSize)]);
				itemNode.coolDown = 1; // TODO

				// set modifiers
				Modifiers modifiers;
				modifiers.duration = (int)(Single)((Godot.Collections.Dictionary)itemDict["modifiers"])[nameof(Modifiers.duration)];
				modifiers.stamina = GetModifier(itemDict, nameof(Modifiers.stamina));
				modifiers.intellect = GetModifier(itemDict, nameof(Modifiers.intellect));
				modifiers.agility = GetModifier(itemDict, nameof(Modifiers.agility));
				modifiers.hpMax = GetModifier(itemDict, nameof(Modifiers.hpMax));
				modifiers.manaMax = GetModifier(itemDict, nameof(Modifiers.manaMax));
				modifiers.maxDamage = GetModifier(itemDict, nameof(Modifiers.maxDamage));
				modifiers.minDamage = GetModifier(itemDict, nameof(Modifiers.minDamage));
				modifiers.regenTime = GetModifier(itemDict, nameof(Modifiers.regenTime));
				modifiers.armor = GetModifier(itemDict, nameof(Modifiers.armor));
				modifiers.weaponRange = GetModifier(itemDict, nameof(Modifiers.weaponRange));
				modifiers.weaponSpeed = GetModifier(itemDict, nameof(Modifiers.weaponSpeed));
				itemNode.modifiers = modifiers;

				// set use
				Use use;
				use.hp = (int)(Single)((Godot.Collections.Dictionary)itemDict["use"])[nameof(Use.hp)];
				use.mana = (int)(Single)((Godot.Collections.Dictionary)itemDict["use"])[nameof(Use.mana)];
				itemNode.use = use;

				// add to cache
				itemData.Add(itemName, itemNode);
			}
		}
		public static ModifierNode GetModifier(Godot.Collections.Dictionary itemDict, string attribute)
		{
			Godot.Collections.Dictionary modifierDict = (Godot.Collections.Dictionary)
				((Godot.Collections.Dictionary)itemDict["modifiers"])[attribute];
			ModifierNode modifierNode;

			modifierNode.type = (StatModifier.StatModType)
				Enum.Parse(typeof(StatModifier.StatModType), (string)modifierDict["type"]);
			modifierNode.value = (float)((Single)modifierDict["value"]);

			return modifierNode;
		}
		public static ItemNode GetItemData(string worldName) { return itemData[worldName]; }
		public static bool HasItem(string nameCheck) { return itemData.ContainsKey(nameCheck); }
	}
}