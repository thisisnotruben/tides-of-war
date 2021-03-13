using System;
using Godot;
namespace Game.Database
{
	public class SpellDB : AbstractDB<SpellDB.SpellData>
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

		public SpellDB(string path) : base(path) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string spellName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[spellName];

				data.Add(spellName, new SpellData(
					icon: IconDB.GetIcon(dict[nameof(SpellData.icon)].ToString().ToInt()),
					type: (SpellTypes)Enum.Parse(typeof(SpellTypes), dict[nameof(SpellData.type)].ToString()),
					level: dict[nameof(SpellData.level)].ToString().ToInt(),
					goldCost: dict[nameof(SpellData.goldCost)].ToString().ToInt(),
					blurb: dict[nameof(SpellData.blurb)].ToString(),
					range: dict[nameof(SpellData.range)].ToString().ToInt(),
					coolDown: dict[nameof(SpellData.coolDown)].ToString().ToInt(),
					ignoreArmor: (bool)dict[nameof(SpellData.ignoreArmor)],
					requiresTarget: (bool)dict[nameof(SpellData.requiresTarget)],
					spellEffect: dict[nameof(SpellData.spellEffect)].ToString(),
					sound: dict[nameof(SpellData.sound)].ToString(),
					characterAnim: dict[nameof(SpellData.characterAnim)].ToString(),
					stackSize: 1,
					manaCost: dict[nameof(SpellData.manaCost)].ToString().ToInt()
				));
			}
		}
	}
}