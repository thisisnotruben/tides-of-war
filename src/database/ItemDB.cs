using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Game.Database
{
    public static class ItemDB
    {
        private static readonly string DB_PATH = "res://data/ItemDB.xml";
        private static readonly string[] typeTags = { "Weapon", "Armor", "Potion", "Food", "Misc" };
        private static readonly XMLParser xMLParser = new XMLParser();
        public static Dictionary<string, string> GetItemData(string worldName)
        {
            bool potion = (worldName.ToLower().Contains("potion") ||
                worldName.ToLower().Contains("elixir"));
            Dictionary<string, string> itemData = new Dictionary<string, string>();
            xMLParser.Open(DB_PATH);
            string itemSubType = "";
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                {
                    if (typeTags.Contains(xMLParser.GetNodeName()))
                    {
                        if (itemData.ContainsKey("type"))
                        {
                            itemData["type"] = xMLParser.GetNodeName();
                        }
                        else
                        {
                            itemData.Add("type", xMLParser.GetNodeName());
                        }
                    }
                    else if (xMLParser.GetNodeName().Equals("itemClass"))
                    {
                        // To get Weapon/Potion subTypes
                        itemSubType = xMLParser.GetNamedAttributeValue("name");
                        if (itemData.ContainsKey("subType"))
                        {
                            itemData["subType"] = itemSubType;
                        }
                        else
                        {
                            itemData.Add("subType", itemSubType);
                        }
                    }
                    else if (xMLParser.GetNodeName().Equals("item") &&
                        (xMLParser.GetNamedAttributeValue("name").Equals(worldName) ||
                            (potion && worldName.Contains(xMLParser.GetNamedAttributeValue("name")))))
                    {
                        while (xMLParser.Read() == Error.Ok &&
                            !(xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd &&
                                xMLParser.GetNodeName().Equals("item")))
                        {
                            if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                            {
                                string key = xMLParser.GetNodeName();
                                itemSubType = xMLParser.GetNamedAttributeValueSafe("name"); // To get the potion subType
                                if (potion && (xMLParser.GetAttributeCount() == 0 ||
                                        worldName.ToLower().Contains(xMLParser.GetNamedAttributeValueSafe("name"))) &&
                                    xMLParser.Read() == Error.Ok && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                                {
                                    itemData.Add(key, xMLParser.GetNodeData());
                                    itemData["subType"] = itemSubType;
                                }
                                else if (!potion && xMLParser.Read() == Error.Ok &&
                                    xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                                {
                                    itemData.Add(key, xMLParser.GetNodeData());
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return itemData;
        }
        public static string GetItemMaterial(string worldName)
        {
            return GetItemData(worldName)["material"];
        }
        public static bool HasItem(string nameCheck)
        {
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element &&
                    xMLParser.GetNodeName().Equals("item") &&
                    xMLParser.GetNamedAttributeValue("name").Equals(nameCheck))
                {
                    return true;
                }
            }
            return false;
        }
    }
}