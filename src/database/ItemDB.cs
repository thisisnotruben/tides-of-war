using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
    public static class ItemDB
    {
        public struct ItemNode
        {
            public string type;
            public string subType;
            public AtlasTexture icon;
            public int level;
            public string material;
            public string description;
        }

        private static Dictionary<string, ItemNode> itemData = new Dictionary<string, ItemNode>();
        private static readonly string DB_PATH = "res://data/item.json";

        static ItemDB()
        {
            LoadItemData();
        }

        private static void LoadItemData()
        {
            File file = new File();
            file.Open(DB_PATH, File.ModeFlags.Read);
            JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
            file.Close();
            Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
            foreach (string itemName in rawDict.Keys)
            {
                Godot.Collections.Dictionary itemDict = (Godot.Collections.Dictionary) rawDict[itemName];
                ItemNode itemNode;
                itemNode.type = (string) itemDict[nameof(ItemNode.type)];
                itemNode.subType = (string) itemDict[nameof(ItemNode.subType)];
                itemNode.icon = IconDB.GetIcon((int) ((Single) itemDict[nameof(ItemNode.icon)]));
                itemNode.level = (int) ((Single) itemDict[nameof(ItemNode.level)]);
                itemNode.material = (string) itemDict[nameof(ItemNode.material)];
                // TODO
                itemNode.description = "TODO";
                itemData.Add(itemName, itemNode);
            }
        }
        
        public static ItemNode GetItemData(string worldName)
        {
            return itemData[worldName];
        }
        
        public static string GetItemMaterial(string worldName)
        {
            return itemData[worldName].material;
        }
        public static bool HasItem(string nameCheck)
        {
            return itemData.ContainsKey(nameCheck);
        }
    }
}