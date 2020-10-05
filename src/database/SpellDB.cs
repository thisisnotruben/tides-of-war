using System.Collections.Generic;
using System.Linq;
using System;
using Game.Ability;
using Godot;
namespace Game.Database
{
	public static class SpellDB
	{
		public struct SpellNode
		{
			public AtlasTexture icon;
			public int level, goldCost, range, coolDown, stackSize, manaCost;
			public string type, blurb, spellEffect, sound, characterAnim;
			public float pctDamage;
			public bool ignoreArmor, effectOnTarget, requiresTarget;
			public ItemDB.Modifiers modifiers;
			public ItemDB.Use use;
		}
		public struct SpellMissileNode
		{
			public Shape2D hitBox;
			public AtlasTexture img;
			public Boolean rotate, instantSpawn, reverse;
		}
		private static readonly Dictionary<string, SpellNode> spellData;
		private static readonly Dictionary<string, SpellMissileNode> spellMissileData;
		private static readonly Dictionary<string, PackedScene> spellEffectData;

		static SpellDB()
		{
			spellData = LoadSpellData("res://data/spell.json");
			spellMissileData = LoadSpellMissileData("res://data/missileSpell.json");
			spellEffectData = LoadSpellEffects("res://src/spell/spellEffect/");
		}
		private static Dictionary<string, SpellNode> LoadSpellData(string path)
		{
			File file = new File();
			if (!file.FileExists(path))
			{
				return null;
			}

			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, SpellNode> spellData = new Dictionary<string, SpellNode>();

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
				spellNode.spellEffect = (string)itemDict[nameof(SpellNode.spellEffect)];
				spellNode.sound = (string)itemDict[nameof(SpellNode.sound)];
				spellNode.characterAnim = (string)itemDict[nameof(SpellNode.characterAnim)];
				spellNode.stackSize = 1;
				spellNode.manaCost = (int)((Single)itemDict[nameof(SpellNode.manaCost)]);

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
			return spellData;
		}
		private static Dictionary<string, SpellMissileNode> LoadSpellMissileData(string path)
		{
			File file = new File();
			if (!file.FileExists(path))
			{
				return null;
			}

			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, SpellMissileNode> spellMissileData = new Dictionary<string, SpellMissileNode>();

			Godot.Collections.Dictionary itemDict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			string filePath = "res://asset/img/missile-spell/{0}.tres";
			foreach (string spellName in rawDict.Keys)
			{
				itemDict = (Godot.Collections.Dictionary)rawDict[spellName];
				SpellMissileNode spellMissileNode;


				spellMissileNode.img = itemDict[nameof(SpellMissileNode.img)] == null
					? null
					: (AtlasTexture)GD.Load(string.Format(filePath, (string)itemDict[nameof(SpellMissileNode.img)]));

				spellMissileNode.hitBox = (Shape2D)GD.Load(string.Format(
					filePath, (string)itemDict[nameof(SpellMissileNode.hitBox)]));

				spellMissileNode.rotate = (bool)itemDict[nameof(SpellMissileNode.rotate)];
				spellMissileNode.instantSpawn = (bool)itemDict[nameof(SpellMissileNode.instantSpawn)];
				spellMissileNode.reverse = (bool)itemDict[nameof(SpellMissileNode.reverse)];

				spellMissileData.Add(spellName, spellMissileNode);
			}
			return spellMissileData;
		}
		private static Dictionary<string, PackedScene> LoadSpellEffects(string path)
		{
			Directory directory = new Directory();
			if (!directory.DirExists(path))
			{
				return null;
			}

			directory.Open(path);
			directory.ListDirBegin(true, true);

			Dictionary<string, PackedScene> spellEffectData = new Dictionary<string, PackedScene>();

			string resourceName = directory.GetNext();
			while (!resourceName.Empty())
			{
				if (!resourceName.Equals("abstract"))
				{
					if (directory.CurrentIsDir())
					{
						LoadSpellEffects(path.PlusFile(resourceName)).ToList().ForEach(x => spellEffectData.Add(x.Key, x.Value));
					}
					else
					{
						spellEffectData[resourceName.BaseName()] = (PackedScene)GD.Load(path.PlusFile(resourceName));
					}
				}
				resourceName = directory.GetNext();
			}

			directory.ListDirEnd();
			return spellEffectData;
		}
		public static bool HasSpell(string nameCheck) { return spellData.ContainsKey(nameCheck); }
		public static SpellNode GetSpellData(string worldName) { return spellData[worldName]; }
		public static string[] GetSpellNames() { return spellData.Keys.ToArray(); }

		public static bool HasSpellMissile(string nameCheck) { return spellMissileData.ContainsKey(nameCheck); }
		public static SpellMissileNode GetSpellMissileData(string worldName) { return spellMissileData[worldName]; }

		public static bool HasSpellEffect(string worldName)
		{
			return HasSpell(worldName) ? spellEffectData.ContainsKey(GetSpellData(worldName).spellEffect) : false;
		}
		public static SpellEffect GetSpellEffect(string worldName) { return (SpellEffect)spellEffectData[worldName].Instance(); }
	}
}