using Game.Actor.Stat;
using System;
namespace Game.Database
{
	public class ModDB : AbstractDB<ModDB.Modifiers>
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

		public static readonly ModDB Instance = new ModDB();

		public ModDB() : base(PathManager.modifier) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
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
		}
		public static ModifierNode GetModifier(Godot.Collections.Dictionary mainDict, string attribute)
		{
			Godot.Collections.Dictionary dict = (Godot.Collections.Dictionary)mainDict[attribute];

			return new ModifierNode(
				type: (StatModifier.StatModType)Enum.Parse(typeof(StatModifier.StatModType), (string)dict["type"]),
				value: (float)dict["value"]
			);
		}
	}
}