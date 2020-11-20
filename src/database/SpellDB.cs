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

		public static readonly SpellDB Instance = new SpellDB();

		public SpellDB() : base(PathManager.spell) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string spellName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[spellName];

				data.Add(spellName, new SpellData(
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
		}
	}
}