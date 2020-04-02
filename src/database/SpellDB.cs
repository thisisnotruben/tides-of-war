using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
    public static class SpellDB
    {
        public struct SpellNode
        {
            public string type;
            public int icon;
            public int level;
            public int spellRange;
            public int coolDown;
            public float percentDamage;
            public bool ignoreArmor;
            public bool effectOnTarget;
            public bool requiresTarget;
            public string description;
        }
        
        private static Dictionary<string, SpellNode> spellData = new Dictionary<string, SpellNode>();
        private static readonly string DB_PATH = "res://data/spell.json";

        static SpellDB()
        {
            LoadSpellData();
        }

        public static void LoadSpellData()
        {
            File file = new File();
            file.Open(DB_PATH, File.ModeFlags.Read);
            JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
            file.Close();
            Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
            foreach (string spellName in rawDict.Keys)
            {
                Godot.Collections.Dictionary itemDict = (Godot.Collections.Dictionary) rawDict[spellName];
                SpellNode spellNode;
                spellNode.type = (string) itemDict[nameof(SpellNode.type)];
                spellNode.icon = (int) ((Single) itemDict[nameof(SpellNode.icon)]);
                spellNode.level = (int) ((Single) itemDict[nameof(SpellNode.level)]);
                spellNode.spellRange = (int) ((Single) itemDict[nameof(SpellNode.spellRange)]);
                spellNode.coolDown = (int) ((Single) itemDict[nameof(SpellNode.coolDown)]);
                spellNode.percentDamage = (float) ((Single) itemDict[nameof(SpellNode.percentDamage)]);
                spellNode.ignoreArmor = (bool) itemDict[nameof(SpellNode.ignoreArmor)];
                spellNode.effectOnTarget = (bool) itemDict[nameof(SpellNode.effectOnTarget)];
                spellNode.requiresTarget = (bool) itemDict[nameof(SpellNode.requiresTarget)];
                spellNode.description = (string) itemDict[nameof(SpellNode.description)];
                spellData.Add(spellName, spellNode);
            }
        }

        public static SpellNode GetSpellData(string worldName)
        {
            return spellData[worldName];
        }
        public static bool HasSpell(string nameCheck)
        {
            return spellData.ContainsKey(nameCheck);
        }
    }
}