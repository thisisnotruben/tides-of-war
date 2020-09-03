using System.Collections.Generic;
using Game.Database;
using Game.Actor;
using Game.Actor.Stat;
using Godot;
namespace Game.ItemPoto
{
	public class Commodity : Node
	{
		private static Dictionary<Character, Dictionary<string, SceneTreeTimer>> cooldowns =
			new Dictionary<Character, Dictionary<string, SceneTreeTimer>>();

		private Dictionary<CharacterStat, StatModifier> modifiers;
		public string worldName;
		private Character character;

		public Commodity Init(string worldName)
		{
			this.worldName = worldName;
			modifiers = CreateModifiers();
			return this;
		}
		private static ItemDB.ModifierNode[] GetModifiers(string worldName)
		{
			ItemDB.Modifiers m = PickableDB.GetModifiers(worldName);
			return new ItemDB.ModifierNode[] { m.stamina, m.intellect, m.agility, m.hpMax,
				m.manaMax, m.maxDamage, m.minDamage, m.regenTime, m.armor, m.weaponRange, m.weaponSpeed };
		}
		public static string GetDescription(string worldName)
		{
			ItemDB.ModifierNode[] mods = GetModifiers(worldName);
			// array in the same order as in 'GetModifiers;
			string[] modNames = new string[] { "Stamina", "Intellect", "Agility", "hpMax",
				"manaMax", "maxDamage", "minDamage", "regenTime", "armor", "weaponRange", "weaponSpeed" };

			int coolDown, level, duration;
			ItemDB.Use use;
			string blurb;
			if (SpellDB.HasSpell(worldName))
			{
				SpellDB.SpellNode spellNode = SpellDB.GetSpellData(worldName);
				coolDown = spellNode.coolDown;
				level = spellNode.level;
				duration = spellNode.modifiers.duration;
				use = spellNode.use;
				blurb = spellNode.blurb;
			}
			else
			{
				ItemDB.ItemNode itemNode = ItemDB.GetItemData(worldName);
				coolDown = itemNode.coolDown;
				level = itemNode.level;
				duration = itemNode.modifiers.duration;
				use = itemNode.use;
				blurb = itemNode.blurb;
			}

			string text = $"Name: {worldName}\nLevel: {level}\n";
			if (coolDown > 0)
			{
				text += $"Cooldown: {coolDown} sec.\n";
			}

			// set modifier text
			string modText = "";
			for (int i = 0; i < mods.Length; i++)
			{
				if ((int)mods[i].value == 0)
				{
					continue;
				}

				modText += modNames[i] + ": ";

				if (mods[i].type == StatModifier.StatModType.PERCENT_ADD
				|| mods[i].type == StatModifier.StatModType.PERCENT_MUL)
				{
					modText += $"{((mods[i].type == StatModifier.StatModType.PERCENT_ADD) ? "+" : "x")}{(int)(mods[i].value * 100f)}%\n";
				}
				else
				{
					modText += $"+{(int)mods[i].value}\n";
				}
			}
			if (!modText.Empty())
			{
				text += $"Duration: {duration} sec.\n" + modText;
			}

			// set use text
			if (use.hp != 0)
			{
				text += $"Hp: +{use.hp}\n";
			}
			if (use.mana != 0)
			{
				text += $"Mana: +{use.mana}\n";
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
		public static bool IsCoolingDown(Character character, string worldName)
		{
			if (cooldowns.ContainsKey(character) && cooldowns[character].ContainsKey(worldName))
			{
				if (cooldowns[character][worldName].TimeLeft == 0.0f)
				{
					cooldowns[character].Remove(worldName);
					return true;
				}
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
		public bool AddCooldown(float cooldownSec)
		{
			if (IsCoolingDown(character, worldName))
			{
				return false;
			}

			SceneTreeTimer cooldown = GetTree().CreateTimer(cooldownSec, false);
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
		public async virtual void Start()
		{
			if (IsCoolingDown(character, worldName))
			{
				return;
			}

			ItemDB.Use use;
			int duration, coolDown;
			if (SpellDB.HasSpell(worldName))
			{
				SpellDB.SpellNode spellNode = SpellDB.GetSpellData(worldName);
				use = spellNode.use;
				duration = spellNode.modifiers.duration;
				coolDown = spellNode.coolDown;
			}
			else
			{
				ItemDB.ItemNode itemNode = ItemDB.GetItemData(worldName);
				use = itemNode.use;
				duration = itemNode.modifiers.duration;
				coolDown = itemNode.coolDown;
			}

			AddCooldown(coolDown);

			if (use.hp != 0)
			{
				character.hp += use.hp;
			}
			if (use.mana != 0)
			{
				character.mana += use.mana;
			}

			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.AddModifier(modifiers[stat]);
			}

			if (duration > 0)
			{
				await ToSignal(GetTree().CreateTimer(duration, false), "timeout");
				Exit();
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