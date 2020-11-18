using Godot;
using Game.Actor.Stat;
using System;
using System.Collections.Generic;
namespace Game.Database
{
	public static class ModUseDB
	{
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

		private static Dictionary<string, Modifiers> modData;
		private static Dictionary<string, Use> useData;

		public static void Init()
		{
			modData = LoadModData(PathManager.modifier);
			useData = LoadUseData(PathManager.use);
		}
		private static Dictionary<string, Modifiers> LoadModData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, Modifiers> data = new Dictionary<string, Modifiers>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new Modifiers(
					durationSec: (int)(Single)dict[nameof(Modifiers.durationSec)],
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
				));
			}
			return data;
		}
		private static Dictionary<string, Use> LoadUseData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, Use> data = new Dictionary<string, Use>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new Use(
					totalSec: (int)(Single)dict[nameof(Use.totalSec)],
					repeatSec: (int)(Single)dict[nameof(Use.repeatSec)],
					hp: GetModifier(dict, nameof(Use.hp)),
					mana: GetModifier(dict, nameof(Use.mana)),
					damage: GetModifier(dict, nameof(Use.damage))
				));
			}
			return data;
		}
		private static ModifierNode GetModifier(Godot.Collections.Dictionary mainDict, string attribute)
		{
			Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)mainDict[attribute];

			return new ModifierNode(
				type: (StatModifier.StatModType)Enum.Parse(typeof(StatModifier.StatModType), (string)dict["type"]),
				value: (float)dict["value"]
			);
		}
		public static bool HasMod(string worldName) { return modData.ContainsKey(worldName); }
		public static Modifiers GetMod(string worldName) { return modData[worldName]; }
		public static bool HasUse(string worldName) { return useData.ContainsKey(worldName); }
		public static Use GetUse(string worldName) { return useData[worldName]; }
	}
}