using System.Collections.Generic;
using System;
using Game.Database;
using Game.Actor;
using Game.Actor.Doodads;
using Game.Actor.Stat;
using Godot;
using GC = Godot.Collections;
namespace Game.GameItem
{
	public class Commodity : WorldObject, ISerializable
	{
		private Dictionary<CharacterStat, StatModifier> modifiers;
		protected Character character;
		protected Timer useTimer = new Timer(), durTimer = new Timer();
		protected int useCount = 0;

		public override void _Ready()
		{
			durTimer.OneShot = true;
			AddChild(useTimer);
			AddChild(durTimer);
			durTimer.Connect("timeout", this, nameof(Exit));
		}
		public virtual Commodity Init(Character character, string worldName)
		{
			this.worldName = worldName;
			this.character = character;
			modifiers = CreateModifiers();
			return this;
		}
		private static ModDB.ModifierNode[] GetModifiers(string worldName)
		{
			ModDB.Modifiers m = Globals.modDB.HasData(worldName)
				? Globals.modDB.GetData(worldName)
				: null;

			return m == null
				? new ModDB.ModifierNode[] { }
				: new ModDB.ModifierNode[] { m.stamina, m.intellect, m.agility, m.hpMax,
				m.manaMax, m.maxDamage, m.minDamage, m.regenTime, m.armor, m.weaponRange, m.weaponSpeed };
		}
		public static string GetDescription(string worldName)
		{
			int duration = Globals.modDB.HasData(worldName)
				? Globals.modDB.GetData(worldName).durationSec
				: 0,
			coolDown, level;

			string blurb;
			if (Globals.spellDB.HasData(worldName))
			{
				SpellDB.SpellData spellData = Globals.spellDB.GetData(worldName);
				coolDown = spellData.coolDown;
				level = spellData.level;
				blurb = spellData.blurb;
			}
			else
			{
				ItemDB.ItemData itemData = Globals.itemDB.GetData(worldName);
				coolDown = itemData.coolDown;
				level = itemData.level;
				blurb = itemData.blurb;
			}

			// statement lambda for getting representational data from modifiers nodes
			Func<ModDB.ModifierNode[], string[], string> GetModifierDescriptions = (m, n) =>
			{
				string description = "";
				int value;
				for (int i = 0; i < m.Length; i++)
				{
					if (m[i].value == 0.0f)
					{
						continue;
					}

					value = (int)m[i].value;
					description += n[i] + ": ";

					if (m[i].type == StatModifier.StatModType.PERCENT_ADD
					|| m[i].type == StatModifier.StatModType.PERCENT_MUL)
					{
						description += (value > 0) ? "+" : "-";
						if (m[i].type == StatModifier.StatModType.PERCENT_MUL)
						{
							description += "x";
						}
						description += (int)Math.Abs((m[i].value * 100f)) + "%\n";
					}
					else
					{
						description += value.ToString("+#;-#;0") + "\n";
					}
				}
				return description;
			};

			string text = $"Level: {level}\n";
			if (coolDown > 0)
			{
				text += $"Cooldown: {coolDown} sec.\n";
			}

			// set mod text | mod name array in the same order as in 'GetModifiers;
			string modText = GetModifierDescriptions(GetModifiers(worldName),
				new string[] { "Stamina", "Intellect", "Agility", "hpMax", "manaMax", "maxDamage",
				"minDamage", "regenTime", "armor", "weaponRange", "weaponSpeed" });

			if (!modText.Empty())
			{
				text += $"Duration: {duration} sec.\n" + modText;
			}

			if (Globals.useDB.HasData(worldName))
			{
				UseDB.Use use = Globals.useDB.GetData(worldName);
				// set use text
				modText = GetModifierDescriptions(
					new ModDB.ModifierNode[] { use.hp, use.mana, use.damage },
					new string[] { "Hp", "Mana", "Damage" });

				if (!modText.Empty())
				{
					text += $"Repeat: {duration} sec.\n" + modText;
				}
			}

			// set blurb text
			if (!blurb.Empty())
			{
				text += blurb;
			}

			return text;
		}
		private Dictionary<CharacterStat, StatModifier> CreateModifiers()
		{
			Dictionary<CharacterStat, StatModifier> modifierDict = new Dictionary<CharacterStat, StatModifier>();
			ModDB.ModifierNode[] mods = GetModifiers(worldName);

			StatManager s = character.stats;
			// array in the same order as in 'GetModifiers;
			CharacterStat[] stats = new CharacterStat[] { s.stamina, s.intellect, s.agility, s.hpMax,
				s.manaMax, s.maxDamage, s.minDamage, s.regenTime, s.armor, s.weaponRange, s.weaponSpeed};

			for (int i = 0; i < mods.Length; i++)
			{
				if ((int)mods[i].value != 0)
				{
					modifierDict.Add(stats[i], new StatModifier(mods[i].value, mods[i].type, this));
				}
			}
			return modifierDict;
		}
		public virtual void ArmUse(bool start)
		{
			if (Globals.useDB.HasData(worldName))
			{
				UseDB.Use use = Globals.useDB.GetData(worldName);

				if (start)
				{
					StartUse(use);
				}
				if (use.repeatSec > 0)
				{
					useTimer.Connect("timeout", this, nameof(OnUseTimeout), new GC.Array() { use });
					useTimer.Start(use.repeatSec);
				}
			}
		}
		public virtual void StartUse(UseDB.Use use)
		{
			if (use.hp.value != 0)
			{
				character.hp += (use.hp.type == StatModifier.StatModType.FLAT)
					? (int)use.hp.value
					: (int)Math.Round(use.hp.value * character.stats.hpMax.value);
			}

			if (use.mana.value != 0)
			{
				character.mana += (use.mana.type == StatModifier.StatModType.FLAT)
					? (int)use.mana.value
					: (int)Math.Round(use.mana.value * character.stats.hpMax.value);
			}

			int damage = (int)use.damage.value;
			if (damage != 0)
			{
				character.Harm(damage, Vector2.Zero);
				character.SpawnCombatText(damage.ToString(), CombatText.TextType.HIT);
			}

			useCount++;
		}
		public void OnUseTimeout(UseDB.Use use)
		{
			if (use.repeatSec > 0
			&& Globals.modDB.HasData(worldName)
			&& useCount < Globals.modDB.GetData(worldName).durationSec / use.repeatSec)
			{
				StartUse(use);
			}
			else
			{
				useTimer.Stop();
			}
		}
		public virtual void Start()
		{
			// adds only if there isn't a cooldown of same type
			Globals.cooldownMaster.AddCooldown(character.GetPath(), worldName,
				Globals.spellDB.HasData(worldName)
					? Globals.spellDB.GetData(worldName).coolDown
					: Globals.itemDB.GetData(worldName).coolDown);

			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.AddModifier(modifiers[stat]);
			}

			ArmUse(useCount == 0);

			if (Globals.modDB.HasData(worldName))
			{
				durTimer.Start(Globals.modDB.GetData(worldName).durationSec);
			}
		}
		public virtual void Exit()
		{
			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.RemoveModifier(modifiers[stat]);
			}
		}
		public virtual GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.TIME_LEFT, durTimer.TimeLeft},
				{NameDB.SaveTag.USE_TIME_LEFT, useTimer.TimeLeft},
				{NameDB.SaveTag.USE_COUNT_LEFT, useCount}
			};
		}
		public virtual void Deserialize(GC.Dictionary payload)
		{
			useCount = payload[NameDB.SaveTag.USE_COUNT_LEFT].ToString().ToInt();
			Start();
			useTimer.Start((float)payload[NameDB.SaveTag.USE_TIME_LEFT]);
			durTimer.Start((float)payload[NameDB.SaveTag.TIME_LEFT]);
		}
	}
}