using System;
using System.Collections.Generic;
using Godot;
namespace Game
{
	public static class Stats
	{
		public enum AttackTableType { MELEE, RANGED }
		public struct CharacterStatsNode
		{
			public float stamina, intellect, agility, hpMax,
				manaMax, maxDamage, minDamage, regenTime, armor;
		}
		public struct AttackTableNode { public byte hit, critical, dodge, parry, miss; }

		public const int
			MIN_LEVEL = 1,
			MAX_LEVEL = 10,
			FLEE_DISTANCE = 128,
			XP_INTERVAL = 1_000,
			MAX_XP = MAX_LEVEL * XP_INTERVAL;

		public const float
			HP_MANA_RESPAWN_MIN_LIMIT = 0.3f,
			WEAPON_RANGE_MELEE = 32.0f + 4.0f,
			WEAPON_RANGE_RANGE = 64.0f + 4.0f,
			SPEED = 0.5f,
			MULTIPLIER = 2.6f;
		public static readonly Dictionary<AttackTableType, AttackTableNode> ATTACK_TABLE;

		static Stats()
		{
			ATTACK_TABLE = new Dictionary<AttackTableType, AttackTableNode>();
			AttackTableNode melee;
			melee.hit = 74;
			melee.critical = 78;
			melee.dodge = 85;
			melee.parry = 100;
			melee.miss = 100;
			ATTACK_TABLE.Add(AttackTableType.MELEE, melee);
			AttackTableNode range;
			range.hit = 74;
			range.critical = 78;
			range.dodge = 84;
			range.parry = 89;
			range.miss = 100;
			ATTACK_TABLE.Add(AttackTableType.RANGED, range);
		}
		public static CharacterStatsNode UnitMake(float level, float unitMultiplier)
		{
			float stamina = (3.0f + level) * unitMultiplier;
			float intellect = (2.0f + level) * unitMultiplier;
			float agility = (1.0f + level) * unitMultiplier;

			// derived base values
			float hpMax = (6.0f * level + 24.0f + stamina) * unitMultiplier;
			float manaMax = (6.0f * level + 16.0f + intellect) * unitMultiplier;
			float maxDamage = ((hpMax * 0.225f) - (hpMax * 0.225f / 2.0f)) / 2.0f;
			float minDamage = maxDamage / 2.0f;
			float regenTime = 60.0f - 60.0f * agility * 0.01f;
			float armor = ((stamina + agility) / 2.0f) * ((minDamage + maxDamage) / 2.0f) * 0.01f;

			CharacterStatsNode stats;
			stats.stamina = stamina;
			stats.intellect = intellect;
			stats.agility = agility;
			stats.hpMax = hpMax;
			stats.manaMax = manaMax;
			stats.maxDamage = maxDamage;
			stats.minDamage = minDamage;
			stats.regenTime = regenTime;
			stats.armor = armor;
			return stats;
		}
		public static int HpManaRegenAmount(float level, float unitMultiplier)
		{
			return (int)Math.Round((6.0f * level + 24 + ((3.0f + level) * unitMultiplier)) * unitMultiplier * 0.05f);
		}
		public static int GetXpFromUnitDeath(double level, double unitMultiplier, double winnerLevel)
		{
			return (int)(20 + (level - winnerLevel) * unitMultiplier);
		}
		public static int HealerCost(int playerLevel)
		{
			return (3 * playerLevel + 2) * 4;
		}
		public static int ItemRepairCost(int itemLevel)
		{
			return (3 * itemLevel + 1) * 3;
		}
		public static int GetSpellWorthCost(int spellLevel)
		{
			return (3 * spellLevel + 10) * 6;
		}
		public static int GetSpellManaCost(int spellLevel)
		{
			float manaCost = (6 * spellLevel + 16 + (spellLevel + 2) * MULTIPLIER) * MULTIPLIER * 0.3f;
			manaCost = (manaCost + manaCost * 1.25f) / 2.0f - 9.0f;
			return (int)Math.Round(manaCost);
		}
		public static int GetModifiedRegen(float characterLevel, float unitMultiplier)
		{
			return (int)Math.Round((6.0f * characterLevel + 24.0f + ((3.0 + characterLevel) * unitMultiplier)) * unitMultiplier * 0.05f);
		}
		// TODO
		// public static int GetItemGoldWorth(int itemLevel, WorldObject.WorldTypes itemType, float itemDurability)
		// {
		// 	int gold = 3 * itemLevel + 4;
		// 	switch (itemType)
		// 	{
		// 		case Item.WorldTypes.FOOD:
		// 			gold *= 2;
		// 			break;
		// 		case Item.WorldTypes.HEALING:
		// 		case Item.WorldTypes.MANA:
		// 			gold *= 3;
		// 			break;
		// 		case Item.WorldTypes.STAMINA:
		// 		case Item.WorldTypes.INTELLECT:
		// 		case Item.WorldTypes.AGILITY:
		// 		case Item.WorldTypes.STRENGTH:
		// 		case Item.WorldTypes.DEFENSE:
		// 			gold *= 4;
		// 			break;
		// 		case Item.WorldTypes.WEAPON:
		// 			gold *= 5;
		// 			break;
		// 		case Item.WorldTypes.ARMOR:
		// 			gold *= 6;
		// 			break;
		// 	}
		// 	return (int)Mathf.Round((float)gold * itemDurability);
		// }
		public static Tuple<int, int> GetItemStats(int itemLevel, WorldObject.WorldTypes itemType, WorldObject.WorldTypes itemSubType)
		{
			CharacterStatsNode stats = UnitMake(itemLevel, MULTIPLIER);
			double minValue = -1.0;
			double maxValue = -1.0;
			// TODO
			// switch (itemSubType)
			// {
			// 	case Item.WorldTypes.HEALING:
			// 		minValue = stats.hpMax * 0.3;
			// 		maxValue = minValue * 1.25;
			// 		break;
			// 	case Item.WorldTypes.MANA:
			// 		minValue = stats.manaMax * 0.3;
			// 		maxValue = minValue * 1.25;
			// 		break;
			// 	case Item.WorldTypes.STAMINA:
			// 		minValue = stats.stamina * 0.5;
			// 		break;
			// 	case Item.WorldTypes.INTELLECT:
			// 		minValue = stats.intellect * 0.5;
			// 		break;
			// 	case Item.WorldTypes.AGILITY:
			// 		minValue = stats.agility * 0.5;
			// 		break;
			// 	case Item.WorldTypes.STRENGTH:
			// 		minValue = (stats.minDamage + stats.maxDamage) * 0.5;
			// 		break;
			// 	case Item.WorldTypes.DEFENSE:
			// 		minValue = (double)itemLevel;
			// 		break;
			// 	default:
			// 		switch (itemType)
			// 		{
			// 			case Item.WorldTypes.ARMOR:
			// 				minValue = (double)itemLevel;
			// 				break;
			// 			case Item.WorldTypes.WEAPON:
			// 				minValue = stats.minDamage;
			// 				maxValue = stats.maxDamage;
			// 				break;
			// 			case Item.WorldTypes.FOOD:
			// 				maxValue = stats.hpMax * 0.09375;
			// 				minValue = maxValue * 0.5;
			// 				break;
			// 		}
			// 		break;
			// }
			// minValue = Math.Round(minValue);
			// maxValue = Math.Round(maxValue);
			return new Tuple<int, int>((int)minValue, (int)maxValue);
		}
		public static float GetMultiplier(string race)
		{
			float multiplier;
			switch (race)
			{
				case "player":
					multiplier = 2.6f;
					break;
				case "minotaur":
				case "oliphant":
					multiplier = 2.4f;
					break;
				case "human":
				case "orc":
					multiplier = 2.2f;
					break;
				case "gnoll":
				case "goblin":
				case "warthog":
					multiplier = 2.0f;
					break;
				case "critter":
					multiplier = 1.8f;
					break;
				default:
					multiplier = 1.0f;
					break;
			}
			return multiplier;
		}
		public static float GetMultiplier(bool npc, string imgPath)
		{
			float multiplier = MULTIPLIER;
			string[] splittedImgPath = imgPath.Split('/');
			string raceName = imgPath.GetFile().BaseName().Split("-")[0];
			if (npc)
			{
				switch (raceName)
				{
					case "minotaur":
					case "oliphant":
						multiplier = 2.4f;
						break;
					case "human":
					case "orc":
						multiplier = 2.2f;
						break;
					case "gnoll":
					case "goblin":
					case "warthog":
						multiplier = 2.0f;
						break;
					case "critter":
						multiplier = 1.8f;
						break;
				}
			}
			return multiplier;
		}
		public static int CheckLevel(int xp)
		{
			return xp / XP_INTERVAL;
		}
		public static float MapAnimMoveSpeed(float characterCurrAnimSpeed)
		{
			return Mathf.Clamp(1.0f - SPEED * characterCurrAnimSpeed, 0.01f, 1.0f);
		}
	}
}