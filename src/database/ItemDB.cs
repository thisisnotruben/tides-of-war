using System.Collections.Generic;
using System;
using Game.Actor.Stat;
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
			public readonly Modifiers modifiers;
			public readonly Use use;

			public ItemData(ItemType type, Texture icon, int level, int goldCost, int stackSize,
			int coolDown, string blurb, string subType, string material, Modifiers modifiers, Use use)
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
				this.modifiers = modifiers;
				this.use = use;
			}
		}
		public class Modifiers
		{
			public readonly int durationSec;
			public readonly ModifierNode stamina, intellect, agility,
				hpMax, manaMax, maxDamage, minDamage, regenTime,
				armor, weaponRange, weaponSpeed, moveSpeed;

			public Modifiers(int durationSec, ModifierNode stamina, ModifierNode intellect,
			ModifierNode agility, ModifierNode hpMax, ModifierNode manaMax,
			ModifierNode maxDamage, ModifierNode minDamage, ModifierNode regenTime,
			ModifierNode armor, ModifierNode weaponRange, ModifierNode weaponSpeed, ModifierNode moveSpeed)
			{
				this.durationSec = durationSec;
				this.stamina = stamina;
				this.intellect = intellect;
				this.agility = agility;
				this.hpMax = hpMax;
				this.manaMax = manaMax;
				this.maxDamage = maxDamage;
				this.minDamage = minDamage;
				this.regenTime = regenTime;
				this.armor = armor;
				this.weaponRange = weaponRange;
				this.weaponSpeed = weaponSpeed;
				this.moveSpeed = moveSpeed;
			}
		}
		public class ModifierNode
		{
			public readonly StatModifier.StatModType type;
			public readonly float value;

			public ModifierNode(StatModifier.StatModType type, float value)
			{
				this.type = type;
				this.value = value;
			}
		}
		public class Use
		{
			public readonly int totalSec, repeatSec;
			public readonly ModifierNode hp, mana, damage;

			public Use(int totalSec, int repeatSec, ModifierNode hp, ModifierNode mana, ModifierNode damage)
			{
				this.totalSec = totalSec;
				this.repeatSec = repeatSec;
				this.hp = hp;
				this.mana = mana;
				this.damage = damage;
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
					coolDown: (int)((Single)dict[nameof(ItemData.coolDown)]),
					modifiers: new Modifiers(
						durationSec: (int)(Single)((Godot.Collections.Dictionary)dict["modifiers"])[nameof(Modifiers.durationSec)],
						stamina: GetModifier(dict, nameof(Modifiers.stamina)),
						intellect: GetModifier(dict, nameof(Modifiers.intellect)),
						agility: GetModifier(dict, nameof(Modifiers.agility)),
						hpMax: GetModifier(dict, nameof(Modifiers.hpMax)),
						manaMax: GetModifier(dict, nameof(Modifiers.manaMax)),
						maxDamage: GetModifier(dict, nameof(Modifiers.maxDamage)),
						minDamage: GetModifier(dict, nameof(Modifiers.minDamage)),
						regenTime: GetModifier(dict, nameof(Modifiers.regenTime)),
						armor: GetModifier(dict, nameof(Modifiers.armor)),
						weaponRange: GetModifier(dict, nameof(Modifiers.weaponRange)),
						weaponSpeed: GetModifier(dict, nameof(Modifiers.weaponSpeed)),
						moveSpeed: GetModifier(dict, nameof(Modifiers.moveSpeed))
					),
					use: new Use(
						totalSec: (int)(Single)((Godot.Collections.Dictionary)dict["use"])[nameof(Use.totalSec)],
						repeatSec: (int)(Single)((Godot.Collections.Dictionary)dict["use"])[nameof(Use.repeatSec)],
						hp: GetModifier(dict, nameof(Use.hp), "use"),
						mana: GetModifier(dict, nameof(Use.mana), "use"),
						damage: GetModifier(dict, nameof(Use.damage), "use")
					)
				));
			}
		}
		public static ModifierNode GetModifier(Godot.Collections.Dictionary mainDict, string attribute, string primaryTag = "modifiers")
		{
			Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)
				((Godot.Collections.Dictionary)mainDict[primaryTag])[attribute];

			return new ModifierNode(
				type: (StatModifier.StatModType)Enum.Parse(typeof(StatModifier.StatModType), (string)dict["type"]),
				value: (float)((Single)dict["value"])
			);
		}
		public static ItemData GetItemData(string worldName) { return itemData[worldName]; }
		public static bool HasItem(string nameCheck) { return itemData.ContainsKey(nameCheck); }
	}
}