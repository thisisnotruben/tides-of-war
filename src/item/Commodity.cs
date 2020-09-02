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
			ItemDB.Modifiers m = ItemDB.GetItemData(worldName).modifiers;
			return new ItemDB.ModifierNode[] { m.stamina, m.intellect, m.agility, m.hpMax,
				m.manaMax, m.maxDamage, m.minDamage, m.regenTime, m.armor, m.weaponRange, m.weaponSpeed };
		}
		public static string GetDescription(string worldName)
		{
			ItemDB.ModifierNode[] mods = GetModifiers(worldName);
			// array in the same order as in 'GetModifiers;
			string[] modNames = new string[] { "Stamina", "Intellect", "Agility", "hpMax",
				"manaMax", "maxDamage", "minDamage", "regenTime", "armor", "weaponRange", "weaponSpeed" };
			ItemDB.ItemNode itemNode = ItemDB.GetItemData(worldName);

			string text = $"Name: {worldName}\nLevel: {itemNode.level}\n";
			if (itemNode.coolDown > 0)
			{
				text += $"Cooldown: {itemNode.coolDown} sec.\n";
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
				text += $"Duration: {itemNode.modifiers.duration} sec.\n" + modText;
			}

			// set use text
			if (itemNode.use.hp != 0)
			{
				text += $"Hp: +{itemNode.use.hp}\n";
			}
			if (itemNode.use.mana != 0)
			{
				text += $"Mana: +{itemNode.use.mana}\n";
			}

			// set blurb text
			if (!itemNode.blurb.Empty())
			{
				text += itemNode.blurb;
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

			ItemDB.ItemNode itemNode = ItemDB.GetItemData(worldName);
			AddCooldown(itemNode.coolDown);

			if (itemNode.use.hp != 0)
			{
				character.hp += itemNode.use.hp;
			}
			if (itemNode.use.mana != 0)
			{
				character.mana += itemNode.use.mana;
			}

			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.AddModifier(modifiers[stat]);
			}

			if (itemNode.modifiers.duration > 0)
			{
				await ToSignal(GetTree().CreateTimer(itemNode.modifiers.duration, false), "timeout");
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