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
			public int level, goldCost, stackSize, coolDown;
			public string blurb, subType, material;
			public Modifiers modifiers;
			public Use use;
		}
		public struct Modifiers
		{
			public int durationSec;
			public ModifierNode stamina, intellect, agility,
				hpMax, manaMax, maxDamage, minDamage, regenTime,
				armor, weaponRange, weaponSpeed, moveSpeed;
		}
		public struct ModifierNode
		{
			public StatModifier.StatModType type;
			public float value;
		}
		public struct Use
		{
			public int totalSec, repeatSec;
			public ModifierNode hp, mana, damage;
		}
		private static Dictionary<string, ItemNode> itemData = new Dictionary<string, ItemNode>();
		private const string DB_PATH = "res://data/item.json";

		static ItemDB() { LoadItemData(); }
		public static void Init() { }
		private static void LoadItemData()
		{
			File file = new File();
			file.Open(DB_PATH, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary itemDict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string itemName in rawDict.Keys)
			{
				itemDict = (Godot.Collections.Dictionary)rawDict[itemName];
				ItemNode itemNode;
				itemNode.type = (ItemType)Enum.Parse(typeof(ItemType), (string)itemDict[nameof(ItemNode.type)]);
				itemNode.icon = IconDB.GetIcon((int)((Single)itemDict[nameof(ItemNode.icon)]));
				itemNode.level = (int)((Single)itemDict[nameof(ItemNode.level)]);
				itemNode.goldCost = (int)((Single)itemDict[nameof(ItemNode.goldCost)]);
				itemNode.blurb = (string)itemDict[nameof(ItemNode.blurb)];
				itemNode.subType = (string)itemDict[nameof(ItemNode.subType)];
				itemNode.material = (string)itemDict[nameof(ItemNode.material)];
				itemNode.stackSize = (int)((Single)itemDict[nameof(ItemNode.stackSize)]);
				itemNode.coolDown = (int)((Single)itemDict[nameof(ItemNode.coolDown)]);

				// set modifiers
				Modifiers modifiers;
				modifiers.durationSec = (int)(Single)((Godot.Collections.Dictionary)itemDict["modifiers"])[nameof(Modifiers.durationSec)];
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
				modifiers.moveSpeed = GetModifier(itemDict, nameof(Modifiers.moveSpeed));
				itemNode.modifiers = modifiers;

				// set use

				Use use;
				use.totalSec = (int)(Single)((Godot.Collections.Dictionary)itemDict["use"])[nameof(Use.totalSec)];
				use.repeatSec = (int)(Single)((Godot.Collections.Dictionary)itemDict["use"])[nameof(Use.repeatSec)];
				use.hp = GetModifier(itemDict, nameof(Use.hp), "use");
				use.mana = GetModifier(itemDict, nameof(Use.mana), "use");
				use.damage = GetModifier(itemDict, nameof(Use.damage), "use");
				itemNode.use = use;

				// add to cache
				itemData.Add(itemName, itemNode);
			}
		}
		public static ModifierNode GetModifier(Godot.Collections.Dictionary itemDict, string attribute, string primaryTag = "modifiers")
		{
			Godot.Collections.Dictionary modifierDict = (Godot.Collections.Dictionary)
				((Godot.Collections.Dictionary)itemDict[primaryTag])[attribute];
			ModifierNode modifierNode;

			modifierNode.type = (StatModifier.StatModType)
				Enum.Parse(typeof(StatModifier.StatModType), (string)modifierDict["type"]);
			modifierNode.value = (float)((Single)modifierDict["value"]);

			// TODO: rethink this
			// if (primaryTag.Equals("use")
			// && (modifierNode.type == StatModifier.StatModType.PERCENT_ADD
			// || (modifierNode.type != StatModifier.StatModType.FLAT && attribute.Equals(nameof(Use.damage)))))
			// {
			// 	// this doesn't make sense for one-time use attributes so throw exception
			// 	throw new Exception($"{nameof(Use)} type cannot be : {modifierNode.type}");
			// }

			return modifierNode;
		}
		public static ItemNode GetItemData(string worldName) { return itemData[worldName]; }
		public static bool HasItem(string nameCheck) { return itemData.ContainsKey(nameCheck); }
	}
}