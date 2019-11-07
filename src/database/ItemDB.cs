using Godot;
using System.Collections.Generic;

namespace Game.Database
{
    public static class ItemDB
    {
        private static readonly string DB_PATH = "res://src/database/data/ItemDB.xml";
        private static readonly XMLParser xMLParser = new XMLParser();

        public static Dictionary<string, string> GetItemData(string worldName)
        {
            bool potion = (worldName.ToLower().Contains("potion")
                || worldName.ToLower().Contains("elixir"));
            Dictionary<string, string> itemData = new Dictionary<string, string>();
            xMLParser.Open(DB_PATH);
            string itemType = "";
            string itemSubType = "";
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                {
                    if (xMLParser.GetNodeName().Equals("itemClass"))
                    {
                        // To get Weapon/Potion subTypes
                        itemSubType = xMLParser.GetNamedAttributeValue("name");
                    }
                    else if (xMLParser.GetNodeName().Equals("item")
                    && (xMLParser.GetNamedAttributeValue("name").Equals(worldName)
                    || (potion && worldName.Contains(xMLParser.GetNamedAttributeValue("name")))))
                    {
                        itemData.Add("type", itemType);
                        itemData.Add("subType", itemSubType);
                        while (xMLParser.Read() == Error.Ok
                        && (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                        && xMLParser.GetNodeName().Equals("item")))
                        {
                            if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                            {
                                string key = xMLParser.GetNodeName();
                                itemSubType = xMLParser.GetNamedAttributeValueSafe("name"); // To get the potion subType
                                if (potion && (xMLParser.GetAttributeCount() == 0
                                || worldName.ToLower().Contains(xMLParser.GetNamedAttributeValueSafe("name")))
                                && xMLParser.Read() == Error.Ok && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                                {
                                    itemData.Add(key, xMLParser.GetNodeData());
                                    itemData["subType"] = itemSubType;
                                }
                                else if (!potion && xMLParser.Read() == Error.Ok
                                && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                                {
                                    itemData.Add(key, xMLParser.GetNodeData());
                                }
                            }
                        }
                        break;
                    }
                    itemType = xMLParser.GetNodeName();
                }
            }
            return itemData;
        }
        public static string GetItemArmorMaterial(string worldName)
        {
            return GetItemData(worldName)["material"];
        }
        public static bool HasItem(string nameCheck)
        {
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals("itemClass")
                && xMLParser.GetNamedAttributeValue("name").Equals(nameCheck))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

