using System.Collections.Generic;
using System;
using Game.Database;
using Game.Actor;
using Game.Actor.Stat;
using Godot;
using GC = Godot.Collections;
namespace Game.GameItem
{
	public class Commodity : WorldObject, ISerializable
	{
		// characterNodePath: commodityName: timeLeft
		private static readonly Dictionary<string, Dictionary<string, SceneTreeTimer>> cooldowns =
			new Dictionary<string, Dictionary<string, SceneTreeTimer>>();

		private Dictionary<CharacterStat, StatModifier> modifiers;
		protected Character character;
		private Timer useTimer = new Timer(), durTimer = new Timer();
		private int useCount = 0;

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
		public static float GetCoolDown(string rootNodePath, string worldName)
		{
			return IsCoolingDown(rootNodePath, worldName)
				? cooldowns[rootNodePath][worldName].TimeLeft
				: 0.0f;
		}
		public static bool IsCoolingDown(string rootNodePath, string worldName)
		{
			if (cooldowns.ContainsKey(rootNodePath) && cooldowns[rootNodePath].ContainsKey(worldName))
			{
				if (cooldowns[rootNodePath][worldName].TimeLeft == 0.0f)
				{
					cooldowns[rootNodePath].Remove(worldName);
					return false;
				}
				return true;
			}
			return false;
		}
		public static void OnCooldownTimeout(string rootNodePath, string worldName)
		{
			if (cooldowns.ContainsKey(rootNodePath) && cooldowns[rootNodePath].ContainsKey(worldName))
			{
				cooldowns[rootNodePath].Remove(worldName);
				if (cooldowns[rootNodePath].Count == 0)
				{
					cooldowns.Remove(rootNodePath);
				}
			}
		}
		public bool AddCooldown(string rootNodePath, string worldName, float cooldownSec)
		{
			if (IsCoolingDown(rootNodePath, worldName))
			{
				return false;
			}

			SceneTreeTimer cooldown = character.GetTree().CreateTimer(cooldownSec, false);
			cooldown.Connect("timeout", this, nameof(OnCooldownTimeout),
				new GC.Array() { character.GetPath(), worldName });

			if (cooldowns.ContainsKey(rootNodePath))
			{
				cooldowns[rootNodePath].Add(worldName, cooldown);
			}
			else
			{
				cooldowns.Add(rootNodePath, new Dictionary<string, SceneTreeTimer>() { { worldName, cooldown } });
			}
			return true;
		}
		public virtual void StartUse(UseDB.Use use, int durationSec)
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
				character.Harm((int)use.damage.value, Vector2.Zero);
			}

			// iterate through use count
			if (useTimer.IsStopped() && use.repeatSec > 0)
			{
				useTimer.WaitTime = use.repeatSec;
				useTimer.Connect("timeout", this, nameof(OnUseTimeout),
					new GC.Array() { use, durationSec });
				useTimer.Start();
			}
		}
		public void OnUseTimeout(UseDB.Use use, int durationSec)
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
			if (IsCoolingDown(character.GetPath(), worldName))
			{
				return;
			}

			int duration = Globals.modDB.HasData(worldName)
				? Globals.modDB.GetData(worldName).durationSec
				: 0,
			coolDown = Globals.spellDB.HasData(worldName)
				? Globals.spellDB.GetData(worldName).coolDown
				: Globals.itemDB.GetData(worldName).coolDown;

			if (coolDown > 0)
			{
				AddCooldown(character.GetPath(), worldName, coolDown);
			}

			foreach (CharacterStat stat in modifiers.Keys)
			{
				stat.AddModifier(modifiers[stat]);
			}

			if (Globals.useDB.HasData(worldName))
			{
				StartUse(Globals.useDB.GetData(worldName), duration);
			}

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
		public GC.Dictionary Serialize()
		{
			// TODO: save
			GC.Dictionary savedCooldowns = new GC.Dictionary();
			foreach (string nodePath in cooldowns.Keys)
			{
				savedCooldowns[nodePath.ToString()] = new GC.Dictionary<string, float>();
				foreach (string commodityName in cooldowns[nodePath].Keys)
				{
					savedCooldowns[commodityName] = cooldowns[nodePath][commodityName].TimeLeft;
				}
			}

			return new GC.Dictionary()
			{
				{NameDB.SaveTag.COOLDOWNS, savedCooldowns}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			// TODO: save
			GC.Dictionary savedCooldowns = (GC.Dictionary)payload[NameDB.SaveTag.COOLDOWNS];
			GC.Dictionary<string, float> specificCooldowns;

			foreach (string nodePath in savedCooldowns.Keys)
			{
				specificCooldowns = (GC.Dictionary<string, float>)savedCooldowns[nodePath];

				foreach (string commodityName in specificCooldowns.Keys)
				{
					AddCooldown(nodePath, commodityName, specificCooldowns[commodityName]);
				}
			}
		}
	}
}