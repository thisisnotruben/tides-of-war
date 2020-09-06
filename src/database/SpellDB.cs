using System.Collections.Generic;
using System.Linq;
using System;
using Godot;
namespace Game.Database
{
	public static class SpellDB
	{
		public struct SpellNode
		{
			public Texture icon;
			public int level, goldCost, range, coolDown, stackSize, manaCost;
			public string type, blurb;
			public float pctDamage;
			public bool ignoreArmor, effectOnTarget, requiresTarget;
			public ItemDB.Modifiers modifiers;
			public ItemDB.Use use;
		}
		private static Dictionary<string, SpellNode> spellData = new Dictionary<string, SpellNode>();
		private const string DB_PATH = "res://data/spell.json";

		static SpellDB() { LoadSpellData(); }
		private static void LoadSpellData()
		{
			File file = new File();
			file.Open(DB_PATH, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary itemDict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string spellName in rawDict.Keys)
			{
				itemDict = (Godot.Collections.Dictionary)rawDict[spellName];
				SpellNode spellNode;
				spellNode.type = (string)itemDict[nameof(SpellNode.type)];
				spellNode.icon = IconDB.GetIcon((int)((Single)itemDict[nameof(SpellNode.icon)]));
				spellNode.level = (int)((Single)itemDict[nameof(SpellNode.level)]);
				spellNode.goldCost = (int)((Single)itemDict[nameof(SpellNode.goldCost)]);
				spellNode.blurb = (string)itemDict[nameof(SpellNode.blurb)];
				spellNode.range = (int)((Single)itemDict[nameof(SpellNode.range)]);
				spellNode.coolDown = (int)((Single)itemDict[nameof(SpellNode.coolDown)]);
				spellNode.pctDamage = (float)((Single)itemDict[nameof(SpellNode.pctDamage)]);
				spellNode.ignoreArmor = (bool)itemDict[nameof(SpellNode.ignoreArmor)];
				spellNode.effectOnTarget = (bool)itemDict[nameof(SpellNode.effectOnTarget)];
				spellNode.requiresTarget = (bool)itemDict[nameof(SpellNode.requiresTarget)];
				spellNode.stackSize = 1;
				spellNode.manaCost = -1; // TODO

				// set modifiers
				ItemDB.Modifiers modifiers;
				modifiers.durationSec = (int)(Single)((Godot.Collections.Dictionary)itemDict["modifiers"])[nameof(ItemDB.Modifiers.durationSec)];
				modifiers.stamina = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.stamina));
				modifiers.intellect = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.intellect));
				modifiers.agility = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.agility));
				modifiers.hpMax = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.hpMax));
				modifiers.manaMax = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.manaMax));
				modifiers.maxDamage = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.maxDamage));
				modifiers.minDamage = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.minDamage));
				modifiers.regenTime = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.regenTime));
				modifiers.armor = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.armor));
				modifiers.weaponRange = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.weaponRange));
				modifiers.weaponSpeed = ItemDB.GetModifier(itemDict, nameof(ItemDB.Modifiers.weaponSpeed));
				spellNode.modifiers = modifiers;

				// set use
				ItemDB.Use use;
				use.repeatSec = (int)(Single)((Godot.Collections.Dictionary)itemDict["use"])[nameof(ItemDB.Use.repeatSec)];
				use.hp = ItemDB.GetModifier(itemDict, nameof(ItemDB.Use.hp), "use");
				use.mana = ItemDB.GetModifier(itemDict, nameof(ItemDB.Use.mana), "use");
				use.damage = ItemDB.GetModifier(itemDict, nameof(ItemDB.Use.damage), "use");
				spellNode.use = use;


				spellData.Add(spellName, spellNode);
			}
		}
		public static SpellNode GetSpellData(string worldName) { return spellData[worldName]; }
		public static bool HasSpell(string nameCheck) { return spellData.ContainsKey(nameCheck); }
		public static string[] GetSpellNames() { return spellData.Keys.ToArray(); }
	}
}