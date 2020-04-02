using System;
using System.Collections.Generic;
using Game.Loot;
using Godot;
namespace Game
{
    public static class Stats
    {
        public struct CharacterStatsNode
        {
            public int stamina;
            public int intellect;
            public int agility;
            public int hpMax;
            public int manaMax;
            public int maxDamage;
            public int minDamage;
            public int regenTime;
            public int armor;
        }

        public const float MULTIPLIER = 2.6f;
        public const byte MAX_LEVEL = 10;
        public const int XP_INTERVAL = 1_000;
        public const int MAX_XP = MAX_LEVEL * XP_INTERVAL;
        public const double HP_MANA_RESPAWN_MIN_LIMIT = 0.3f;
        public const int FLEE_DISTANCE = 128;
        public const float SPEED = 0.5f;
        public const int WEAPON_RANGE_MELEE = 32 + 4;
        public const int WEAPON_RANGE_RANGE = 64 + 4;
        public static readonly Dictionary<string, Dictionary<string, int>> attackTable = new Dictionary<string, Dictionary<string, int>>
        {
            {
            "RANGED",
            new Dictionary<string, int>
            { { "HIT", 74 },
            { "CRITICAL", 78 },
            { "DODGE", 84 },
            { "PARRY", 89 }
            }
            },
            {
            "MELEE",
            new Dictionary<string, int>
            { { "HIT", 74 },
            { "CRITICAL", 78 },
            { "DODGE", 85 },
            { "PARRY", 100 }
            }
            }
        };
        public static readonly Dictionary<string, int> dropTable = new Dictionary<string, int>()
        { { "drop", 60 }, { "questItem", 50 }, { "misc", 50 }, { "foodPotion", 70 }, { "weapon", 60 }, { "armor", 100 }
        };
        public static CharacterStatsNode UnitMake(int level, double unitMultiplier)
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

            CharacterStatsNode characterStatsNode;
            characterStatsNode.stamina = (int)stamina;
            characterStatsNode.intellect = (int)intellect;
            characterStatsNode.agility = (int)agility;
            characterStatsNode.hpMax = (int)hpMax;
            characterStatsNode.manaMax = (int)manaMax;
            characterStatsNode.maxDamage = (int)maxDamage;
            characterStatsNode.minDamage = (int)minDamage;
            characterStatsNode.regenTime = (int)regenTime;
            characterStatsNode.armor = (int)armor;
            return characterStatsNode;
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
        public static int GetItemGoldWorth(int itemLevel, WorldObject.WorldTypes itemType, float itemDurability)
        {
            int gold = 3 * itemLevel + 4;
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
            return (int)Mathf.Round((float)gold * itemDurability);
        }
        public static Tuple<int, int> GetItemStats(int itemLevel, WorldObject.WorldTypes itemType, WorldObject.WorldTypes itemSubType)
        {
            CharacterStatsNode stats = UnitMake(itemLevel, MULTIPLIER);
            double minValue = -1.0;
            double maxValue = -1.0;
            switch (itemSubType)
            {
                case Item.WorldTypes.HEALING:
                    minValue = stats.hpMax * 0.3;
                    maxValue = minValue * 1.25;
                    break;
                case Item.WorldTypes.MANA:
                    minValue = stats.manaMax * 0.3;
                    maxValue = minValue * 1.25;
                    break;
                case Item.WorldTypes.STAMINA:
                    minValue = stats.stamina * 0.5;
                    break;
                case Item.WorldTypes.INTELLECT:
                    minValue = stats.intellect * 0.5;
                    break;
                case Item.WorldTypes.AGILITY:
                    minValue = stats.agility * 0.5;
                    break;
                case Item.WorldTypes.STRENGTH:
                    minValue = (stats.minDamage + stats.maxDamage) * 0.5;
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
                            minValue = stats.minDamage;
                            maxValue = stats.maxDamage;
                            break;
                        case Item.WorldTypes.FOOD:
                            maxValue = stats.hpMax * 0.09375;
                            minValue = maxValue * 0.5;
                            break;
                    }
                    break;
            }
            minValue = Math.Round(minValue);
            maxValue = Math.Round(maxValue);
            return new Tuple<int, int>((int)minValue, (int)maxValue);
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