using System.Collections.Generic;
using System;
using Game.Database;
using Game.Actor;
using Game.Actor.Stat;
using Godot;
namespace Game.ItemPoto
{
	public class Commodity : Node
	{
		// TODO: when changing scenes, null characters will remain...
		private static Dictionary<Character, Dictionary<string, SceneTreeTimer>> cooldowns =
			new Dictionary<Character, Dictionary<string, SceneTreeTimer>>();

		private Dictionary<CharacterStat, StatModifier> modifiers;
		public string worldName;
		protected Character character;
		private Timer useTimer = new Timer(), durTimer = new Timer();
		private int useCount = 0;

		public Commodity(Character character, string worldName)
		{
			this.worldName = worldName;
			this.character = character;
			modifiers = CreateModifiers();

			durTimer.Connect("timeout", this, nameof(Exit));
			durTimer.OneShot = true;
		}
		private static ItemDB.ModifierNode[] GetModifiers(string worldName)
		{
			ItemDB.Modifiers m = PickableDB.GetModifiers(worldName);
			return new ItemDB.ModifierNode[] { m.stamina, m.intellect, m.agility, m.hpMax,
				m.manaMax, m.maxDamage, m.minDamage, m.regenTime, m.armor, m.weaponRange, m.weaponSpeed };
		}
		public static string GetDescription(string worldName)
		{
			int coolDown, level, duration;
			ItemDB.Use use;
			string blurb;
			if (SpellDB.HasSpell(worldName))
			{
				SpellDB.SpellData spellData = SpellDB.GetSpellData(worldName);
				coolDown = spellData.coolDown;
				level = spellData.level;
				duration = spellData.modifiers.durationSec;
				use = spellData.use;
				blurb = spellData.blurb;
			}
			else
			{
				ItemDB.ItemData itemData = ItemDB.GetItemData(worldName);
				coolDown = itemData.coolDown;
				level = itemData.level;
				duration = itemData.modifiers.durationSec;
				use = itemData.use;
				blurb = itemData.blurb;
			}

			// statement lambda for getting representational data from modifiers nodes
			Func<ItemDB.ModifierNode[], string[], string> GetModifierDescriptions = (m, n) =>
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

			string text = $"Name: {worldName}\nLevel: {level}\n";
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

			// set use text
			modText = GetModifierDescriptions(
				new ItemDB.ModifierNode[] { use.hp, use.mana, use.damage },
				new string[] { "Hp", "Mana", "Damage" });

			if (!modText.Empty())
			{
				text += $"Repeat: {duration} sec.\n" + modText;
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
			ItemDB.ModifierNode[] mods = GetModifiers(worldName);

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
		public static float GetCoolDown(Character character, string worldName)
		{
			return IsCoolingDown(character, worldName) ? cooldowns[character][worldName].TimeLeft : 0.0f;
		}
		public static bool IsCoolingDown(Character character, string worldName)
		{
			if (cooldowns.ContainsKey(character) && cooldowns[character].ContainsKey(worldName))
			{
				if (cooldowns[character][worldName].TimeLeft == 0.0f)
				{
					cooldowns[character].Remove(worldName);
				}
				return true;
			}
			return false;
		}
		public static void OnCooldownTimeout(Character character, string worldName)
		{
			if (cooldowns.ContainsKey(character) && cooldowns[character].ContainsKey(worldName))
			{
				cooldowns[character].Remove(worldName);
				if (cooldowns[character].Count == 0)
				{
					cooldowns.Remove(character);
				}
			}
		}
		public bool AddCooldown(string worldName, float cooldownSec)
		{
			if (IsCoolingDown(character, worldName))
			{
				return false;
			}

			SceneTreeTimer cooldown = character.GetTree().CreateTimer(cooldownSec, false);
			cooldown.Connect("timeout", this, nameof(OnCooldownTimeout),
				new Godot.Collections.Array() { character, worldName });

			if (cooldowns.ContainsKey(character))
			{
				cooldowns[character].Add(worldName, cooldown);
			}
			else
			{
				cooldowns.Add(character, new Dictionary<string, SceneTreeTimer>() { { worldName, cooldown } });
			}
			return true;
		}
		public virtual void StartUse(ItemDB.Use use, int durationSec)
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
			if (use.damage.value != 0)
			{
				character.Harm((int)use.damage.value);
			}

			// iterate through use count
			if (useTimer.IsStopped() && use.repeatSec > 0)
			{
				useTimer.WaitTime = use.repeatSec;
				useTimer.Connect("timeout", this, nameof(OnUseTimeout),
					new Godot.Collections.Array() { use, durationSec });
				useTimer.Start();
			}
		}
		public void OnUseTimeout(ItemDB.Use use, int durationSec)
		{
			if (++useCount < durationSec / use.repeatSec)
			{
				StartUse(use, durationSec);
			}
			else
			{
				useTimer.Stop();
			}
		}
		public virtual void Start()
		{
			if (IsCoolingDown(character, worldName))
			{
				return;
			}

			ItemDB.Use use;
			int duration, coolDown;
			if (SpellDB.HasSpell(worldName))
			{
				SpellDB.SpellData spellData = SpellDB.GetSpellData(worldName);
				use = spellData.use;
				duration = spellData.modifiers.durationSec;
				coolDown = spellData.coolDown;
			}
			else
			{
				ItemDB.ItemData itemData = ItemDB.GetItemData(worldName);
				use = itemData.use;
				duration = itemData.modifiers.durationSec;
				coolDown = itemData.coolDown;
			}

			AddCooldown(worldName, coolDown);

			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.AddModifier(modifiers[stat]);
			}

			StartUse(use, duration);

			if (duration > 0)
			{
				// upon timeout, will exit
				durTimer.WaitTime = duration;
				durTimer.Start();
			}
		}
		public virtual void Exit()
		{
			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.RemoveModifier(modifiers[stat]);
			}
		}
	}
}