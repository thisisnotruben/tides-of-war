using Godot;
using System;
using System.Collections.Generic;
using Game.Misc.Loot;

namespace Game
{
    public static class Stats
    {
        public const float MULTIPLIER = 2.6f;
        public const short MAX_LEVEL = 10;
        public const short XP_INTERVAL = 1_000;
        public const short MAX_XP = MAX_LEVEL * XP_INTERVAL;
        public const double HP_MANA_RESPAWN_MIN_LIMIT = 0.3f;
        public const short FLEE_DISTANCE = 128;
        public const float SPEED = 0.5f;
        public const ushort WEAPON_RANGE_MELEE = 32 + 4;
        public const ushort WEAPON_RANGE_RANGE = 64 + 4;
        public static readonly Dictionary<string, Dictionary<string, ushort>> attackTable = new Dictionary<string, Dictionary<string, ushort>>
        {
            {
                "RANGED", new Dictionary<string, ushort>
                {
                    {"HIT", 74},
                    {"CRITICAL", 78},
                    {"DODGE", 84},
                    {"PARRY", 89}
                }
            },
            {
                "MELEE", new Dictionary<string, ushort>
                {
                    {"HIT", 74},
                    {"CRITICAL", 78},
                    {"DODGE", 85},
                    {"PARRY", 100}
                    }
                }
        };
        public static readonly Dictionary<string, ushort> dropTable = new Dictionary<string, ushort>()
        {
            {"drop", 60},
            {"questItem", 50},
            {"misc", 50},
            {"foodPotion", 70},
            {"weapon", 60},
            {"armor", 100}
        };
        public static Dictionary<string, double> UnitMake(double level, double unitMultiplier)
        {
            double stamina = (3.0 + level) * unitMultiplier;
            double intellect = (2 + level) * unitMultiplier;
            double agility = (1.0 + level) * unitMultiplier;
            double hpMax = (6.0 * level + 24 + stamina) * unitMultiplier;
            double manaMax = (6.0 * level + 16 + intellect) * unitMultiplier;
            double maxDamage = ((hpMax * 0.225) - (hpMax * 0.225 / 2.0)) / 2.0;
            double minDamage = maxDamage / 2.0;
            double regenTime = 60 - 60 * agility * 0.01;
            double armor = ((stamina + agility) / 2.0) * ((minDamage + maxDamage) / 2.0) * 0.01;

            return new Dictionary<string, double>()
            {
                {nameof(stamina), stamina},
                {nameof(intellect), intellect},
                {nameof(agility), agility},
                {nameof(hpMax), hpMax},
                {nameof(manaMax), manaMax},
                {nameof(maxDamage), maxDamage},
                {nameof(minDamage), minDamage},
                {nameof(regenTime), regenTime},
                {nameof(armor), armor}
            };
        }
        public static short HpManaRegenAmount(float level, float unitMultiplier)
        {
            return (short)Math.Round((6.0f * level + 24 + ((3.0f + level) * unitMultiplier)) * unitMultiplier * 0.05f);
        }
        public static short GetXpFromUnitDeath(double level, double unitMultiplier, double winnerLevel)
        {
            return (short)(20 + (level - winnerLevel) * unitMultiplier);
        }
        public static short HealerCost(short playerLevel)
        {
            return (short)((3 * playerLevel + 2) * 4);
        }
        public static short ItemRepairCost(short itemLevel)
        {
            return (short)((3 * itemLevel + 1) * 3);
        }
        public static short GetSpellWorthCost(short spellLevel)
        {
            return (short)((3 * spellLevel + 10) * 6);
        }
        public static short GetSpellManaCost(short spellLevel)
        {
            float manaCost = (6 * spellLevel + 16 + (spellLevel + 2) * MULTIPLIER) * MULTIPLIER * 0.3f;
            manaCost = (manaCost + manaCost * 1.25f) / 2.0f - 9.0f;
            return (short)Math.Round(manaCost);
        }
        public static short GetModifiedRegen(float characterLevel, float unitMultiplier)
        {
            return (short)Math.Round((6.0f * characterLevel + 24.0f + ((3.0 + characterLevel) * unitMultiplier)) * unitMultiplier * 0.05f);
        }
        public static short GetItemGoldWorth(short itemLevel, WorldObject.WorldTypes itemType, float itemDurability)
        {
            short gold = (short)(3 * itemLevel + 4);
            switch (itemType)
            {
                case Item.WorldTypes.FOOD:
                    gold *= 2;
                    break;
                case Item.WorldTypes.HEALING:
                case Item.WorldTypes.MANA:
                    gold *= 3;
                    break;
                case Item.WorldTypes.STAMINA:
                case Item.WorldTypes.INTELLECT:
                case Item.WorldTypes.AGILITY:
                case Item.WorldTypes.STRENGTH:
                case Item.WorldTypes.DEFENSE:
                    gold *= 4;
                    break;
                case Item.WorldTypes.WEAPON:
                    gold *= 5;
                    break;
                case Item.WorldTypes.ARMOR:
                    gold *= 6;
                    break;
            }
            return (short)Mathf.Round((float)gold * itemDurability);
        }
        public static Tuple<short, short> GetItemStats(short itemLevel, WorldObject.WorldTypes itemType, WorldObject.WorldTypes itemSubType)
        {
            Dictionary<string, double> stats = UnitMake((double)itemLevel, MULTIPLIER);
            double minValue = -1.0;
            double maxValue = -1.0;
            switch (itemSubType)
            {
                case Item.WorldTypes.HEALING:
                    minValue = stats["hpMax"] * 0.3;
                    maxValue = minValue * 1.25;
                    break;
                case Item.WorldTypes.MANA:
                    minValue = stats["manaMax"] * 0.3;
                    maxValue = minValue * 1.25;
                    break;
                case Item.WorldTypes.STAMINA:
                    minValue = stats["stamina"] * 0.5;
                    break;
                case Item.WorldTypes.INTELLECT:
                    minValue = stats["intellect"] * 0.5;
                    break;
                case Item.WorldTypes.AGILITY:
                    minValue = stats["agility"] * 0.5;
                    break;
                case Item.WorldTypes.STRENGTH:
                    minValue = (stats["minDamage"] + stats["maxDamage"]) * 0.5;
                    break;
                case Item.WorldTypes.DEFENSE:
                    minValue = (double)itemLevel;
                    break;
                default:
                    switch (itemType)
                    {
                        case Item.WorldTypes.ARMOR:
                            minValue = (double)itemLevel;
                            break;
                        case Item.WorldTypes.WEAPON:
                            minValue = stats["minDamage"];
                            minValue = stats["maxDamage"];
                            break;
                        case Item.WorldTypes.FOOD:
                            maxValue = stats["hpMax"] * 0.09375;
                            minValue = maxValue * 0.5;
                            break;
                    }
                    break;
            }
            minValue = Math.Round(minValue);
            maxValue = Math.Round(maxValue);
            return new Tuple<short, short>((short)minValue, (short)maxValue);
        }
        public static float GetMultiplier(bool npc, string imgPath)
        {
            float multiplier = MULTIPLIER;
            string[] splittedImgPath = imgPath.Split('/');
            string raceName = splittedImgPath[splittedImgPath.Length - 2];
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
        public static short CheckLevel(short xp)
        {
            return (short)(xp / XP_INTERVAL);
        }
        public static float MapAnimMoveSpeed(float characterCurrAnimSpeed)
        {
            return Mathf.Clamp(1.0f - SPEED * characterCurrAnimSpeed, 0.01f, 1.0f);
        }
    }
}