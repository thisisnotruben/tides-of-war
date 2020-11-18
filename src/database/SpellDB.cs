using System.Collections.Generic;
using System.Linq;
using System;
using Game.Ability;
using Godot;
namespace Game.Database
{
	public static class SpellDB
	{
		public enum SpellTypes : byte { HIT_HOSTILE, MOD_FRIENDLY, MOD_HOSTILE }

		public class SpellData
		{
			public SpellTypes type;
			public readonly Texture icon;
			public readonly int level, goldCost, range, coolDown, stackSize, manaCost;
			public readonly string blurb, spellEffect, sound, characterAnim;
			public readonly bool ignoreArmor, requiresTarget;

			public SpellData(Texture icon, int level, int goldCost, int range, int coolDown,
			int stackSize, int manaCost, SpellTypes type, string blurb, string spellEffect,
			string sound, string characterAnim, bool ignoreArmor, bool requiresTarget)
			{
				this.icon = icon;
				this.level = level;
				this.goldCost = goldCost;
				this.range = range;
				this.coolDown = coolDown;
				this.stackSize = stackSize;
				this.manaCost = manaCost;
				this.type = type;
				this.blurb = blurb;
				this.spellEffect = spellEffect;
				this.sound = sound;
				this.characterAnim = characterAnim;
				this.ignoreArmor = ignoreArmor;
				this.requiresTarget = requiresTarget;
			}
		}
		public class SpellMissileData
		{
			public readonly Shape2D hitBox;
			public readonly Texture img;
			public readonly Boolean rotate;

			public SpellMissileData(Shape2D hitBox, Texture img, bool rotate)
			{
				this.hitBox = hitBox;
				this.img = img;
				this.rotate = rotate;
			}
		}
		private static Dictionary<string, SpellData> spellData;
		private static Dictionary<string, SpellMissileData> spellMissileData;
		private static Dictionary<string, PackedScene> spellEffectData;

		public static void Init()
		{
			spellData = LoadSpellData(PathManager.spell);
			spellMissileData = LoadSpellMissileData(PathManager.missileSpell);
			spellEffectData = LoadSpellEffects(PathManager.spellEffectDir);
		}
		private static Dictionary<string, SpellData> LoadSpellData(string path)
		{
			File file = new File();
			if (!file.FileExists(path))
			{
				return null;
			}

			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, SpellData> spellData = new Dictionary<string, SpellData>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string spellName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[spellName];

				spellData.Add(spellName, new SpellData(
					icon: IconDB.GetIcon((int)((Single)dict[nameof(SpellData.icon)])),
					type: (SpellTypes)Enum.Parse(typeof(SpellTypes), (string)dict[nameof(SpellData.type)]),
					level: (int)(Single)dict[nameof(SpellData.level)],
					goldCost: (int)(Single)dict[nameof(SpellData.goldCost)],
					blurb: (string)dict[nameof(SpellData.blurb)],
					range: (int)(Single)dict[nameof(SpellData.range)],
					coolDown: (int)(Single)dict[nameof(SpellData.coolDown)],
					ignoreArmor: (bool)dict[nameof(SpellData.ignoreArmor)],
					requiresTarget: (bool)dict[nameof(SpellData.requiresTarget)],
					spellEffect: (string)dict[nameof(SpellData.spellEffect)],
					sound: (string)dict[nameof(SpellData.sound)],
					characterAnim: (string)dict[nameof(SpellData.characterAnim)],
					stackSize: 1,
					manaCost: (int)(Single)dict[nameof(SpellData.manaCost)]
				));
			}
			return spellData;
		}
		private static Dictionary<string, SpellMissileData> LoadSpellMissileData(string path)
		{
			File file = new File();
			if (!file.FileExists(path))
			{
				return null;
			}

			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, SpellMissileData> spellMissileData = new Dictionary<string, SpellMissileData>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			string filePath = "res://asset/img/missile-spell/{0}.tres";
			foreach (string spellName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[spellName];

				spellMissileData.Add(spellName, new SpellMissileData(
					hitBox: (Shape2D)GD.Load(string.Format(filePath, (string)dict[nameof(SpellMissileData.hitBox)])),
					img: dict[nameof(SpellMissileData.img)] == null ? null
						: GD.Load<Texture>(string.Format(filePath, (string)dict[nameof(SpellMissileData.img)])),
					rotate: (bool)dict[nameof(SpellMissileData.rotate)]
				));
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
			string masterDirName = "master",
				sceneExt = "tscn";

			Dictionary<string, PackedScene> spellEffectData = new Dictionary<string, PackedScene>();

			string resourceName = directory.GetNext();
			while (!resourceName.Empty())
			{
				if (directory.CurrentIsDir() && !resourceName.Equals(masterDirName))
				{
					LoadSpellEffects(path.PlusFile(resourceName)).ToList().ForEach(x => spellEffectData.Add(x.Key, x.Value));
				}
				else if (resourceName.Extension().Equals(sceneExt))
				{
					spellEffectData[resourceName.BaseName()] = (PackedScene)GD.Load(path.PlusFile(resourceName));
				}
				resourceName = directory.GetNext();
			}

			directory.ListDirEnd();
			return spellEffectData;
		}
		public static bool HasSpell(string nameCheck) { return spellData.ContainsKey(nameCheck); }
		public static SpellData GetSpellData(string worldName) { return spellData[worldName]; }

		public static bool HasSpellMissile(string nameCheck) { return spellMissileData.ContainsKey(nameCheck); }
		public static SpellMissileData GetSpellMissileData(string worldName) { return spellMissileData[worldName]; }

		public static bool HasSpellEffect(string worldName)
		{
			return HasSpell(worldName) ? spellEffectData.ContainsKey(GetSpellData(worldName).spellEffect) : false;
		}
		public static SpellEffect GetSpellEffect(string worldName) { return (SpellEffect)spellEffectData[worldName].Instance(); }
	}
}