using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
    public static class UnitDB
    {
        public struct UnitNode
        {
            public string name;
            public string img;
            public bool enemy;
            public Vector2 spawnPos;
            public List<Vector2> path;
            public int level;
        }
        private static Dictionary<string, UnitNode> unitData = new Dictionary<string, UnitNode>();

        public static void LoadUnitData(string dbPath)
        {
            // clear out cached database for switching between maps
            unitData.Clear();
            // load & parse data
            File file = new File();
            file.Open(dbPath, File.ModeFlags.Read);
            JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
            file.Close();
            Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
            foreach (string itemName in rawDict.Keys)
            {
                Godot.Collections.Dictionary itemDict = (Godot.Collections.Dictionary) rawDict[itemName];
                UnitNode unitNode;
                unitNode.name = (string) itemDict[nameof(UnitNode.name)];
                unitNode.img = (string) itemDict[nameof(UnitNode.img)];
                unitNode.enemy = (bool) itemDict[nameof(UnitNode.enemy)];
                Godot.Collections.Array spawnPos = (Godot.Collections.Array) itemDict[nameof(UnitNode.spawnPos)];
                unitNode.spawnPos = new Vector2((float)((Single) spawnPos[0]), (float) ((Single) spawnPos[1]));
                unitNode.path = new List<Vector2>();
                foreach (Godot.Collections.Array vectorNode in (Godot.Collections.Array) (itemDict[nameof(UnitNode.path)]))
                {
                    unitNode.path.Add(new Vector2((float)((Single) vectorNode[0]), (float) ((Single) vectorNode[1])));
                }
                unitNode.level = 1; // TODO
                unitData.Add(itemName, unitNode);
            }
        }
        public static UnitNode GetUnitData(string unitEditorName)
        {
            return unitData[unitEditorName];
        }
        public static bool HasUnitData(string nameCheck)
        {
            return unitData.ContainsKey(nameCheck);
        }
    }
}