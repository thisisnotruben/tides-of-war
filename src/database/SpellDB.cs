using System.Collections.Generic;
using System.Linq;
using System;
using Godot;
namespace Game.Database
{
    public static class SpellDB
    {
        public struct SpellNode
        {
            public string type;
            public Texture icon;
            public int level;
            public int goldCost;
            public string blurb;
            public int range;
            public int coolDown;
            public float pctDamage;
            public bool ignoreArmor;
            public bool effectOnTarget;
            public bool requiresTarget;
            public int stackSize;
            public int manaCost;
        }
        private static Dictionary<string, SpellNode> spellData = new Dictionary<string, SpellNode>();
        private static readonly string DB_PATH = "res://data/spell.json";

        static SpellDB()
        {
            LoadSpellData();
        }
        private static void LoadSpellData()
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
                spellNode.icon = IconDB.GetIcon((int) ((Single) itemDict[nameof(SpellNode.icon)]));
                spellNode.level = (int) ((Single) itemDict[nameof(SpellNode.level)]);
                spellNode.goldCost = (int) ((Single) itemDict[nameof(SpellNode.goldCost)]);
                spellNode.blurb = (string) itemDict[nameof(SpellNode.blurb)];
                spellNode.range = (int) ((Single) itemDict[nameof(SpellNode.range)]);
                spellNode.coolDown = (int) ((Single) itemDict[nameof(SpellNode.coolDown)]);
                spellNode.pctDamage = (float) ((Single) itemDict[nameof(SpellNode.pctDamage)]);
                spellNode.ignoreArmor = (bool) itemDict[nameof(SpellNode.ignoreArmor)];
                spellNode.effectOnTarget = (bool) itemDict[nameof(SpellNode.effectOnTarget)];
                spellNode.requiresTarget = (bool) itemDict[nameof(SpellNode.requiresTarget)];
                spellNode.stackSize = 1;
                spellNode.manaCost = -1; // TODO
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
        public static string[] GetSpellNames()
        {
            return spellData.Keys.ToArray();
        }
    }
}