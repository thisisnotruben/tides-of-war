using Godot;
using System;

namespace Game.Database
{
    public static class ItemDB
    {
        public enum potionClasses : byte
        {
            HEALING, MANA, STAMINA, INTELLECT, AGILITY, STRENGTH, DEFENSE,
            MINOR, LESSER, NORMAL, GREATER, SUPERIOR, MAJOR, MASTER, LEGENDARY, MYTHICAL, ANCIENT
        };
        private static string DB_PATH = "res://src/Database/ItemDB.xml";
        private static readonly XMLParser xMLParser = new XMLParser();

        public static AtlasTexture GetItemIcon(string worldName)
        {
            AtlasTexture texture = null;
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && texture == null)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNamedAttributeValueSafe("name").Equals(worldName))
                {
                    string iconID = "";
                    string itemClass = (xMLParser.GetNodeName().Equals("itemClass"))
                        ? xMLParser.GetNamedAttributeValueSafe("name").ToLower()
                        : "";
                    while (xMLParser.Read() == Error.Ok && iconID.Empty()
                    || (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                    && xMLParser.GetNodeName().Equals("itemClass")))
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                        {
                            iconID = xMLParser.GetNodeData();
                        }
                    }
                    texture = (AtlasTexture)GD.Load($"res://asset/img/icon/{itemClass}/{iconID}_icon.res");
                }
            }
            
            return texture;
        }
        public static AtlasTexture GetItemIcon(potionClasses potionClasses, potionClasses potionType)
        {
            AtlasTexture texture = null;
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && texture == null)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals("Potion"))
                {
                    while (xMLParser.Read() == Error.Ok && texture == null)
                    {
                        string itemName = (xMLParser.GetNodeName().Equals("item"))
                            ? xMLParser.GetNamedAttributeValueSafe("name")
                            : "";
                    }
                }
            }
            
            return texture;
        }
        public static int GetItemIconID(string worldName)
        {
            AtlasTexture texture = GetItemIcon(worldName);
            return int.Parse(texture.GetPath().GetFile().Split("_")[0]);
        }
        public static short GetItemLevel(string worldName)
        {
            short itemLevel = -1;
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && itemLevel == -1)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNamedAttributeValueSafe("name").Equals(worldName))
                {
                    while (xMLParser.Read() == Error.Ok && itemLevel == -1
                    || (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                    && xMLParser.GetNodeName().Equals("itemClass")))
                    {
                        itemLevel = short.Parse(xMLParser.GetNodeData());
                    }
                }
            }
            
            return itemLevel;
        }
        public static string GetItemName(WorldObject.WorldTypes type, WorldObject.WorldTypes subType)
        {
            GD.PrintErr("Not implemented");
            return "";
        }
        public static string GetItemName(int iconID)
        {
            string worldName = "";
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && worldName.Empty())
            {
                string itemName = (xMLParser.GetNodeName().Equals("item"))
                    ? xMLParser.GetNamedAttributeValueSafe("name")
                    : "";
                if (!itemName.Empty())
                {
                    while (xMLParser.Read() == Error.Ok && worldName.Empty()
                    || (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                    && xMLParser.GetNodeName().Equals("itemClass")))
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                        && xMLParser.GetNodeName().Equals("iconID")
                        && int.Parse(xMLParser.GetNodeData()) == iconID)
                        {
                            worldName = itemName;
                        }
                    }
                }
            }
            
            return worldName;
        }
        public static string GetItemName(short itemLevel)
        // Not really a good function, the iconID works better
        {
            string worldName = "";
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && worldName.Empty())
            {
                string itemName = (xMLParser.GetNodeName().Equals("item"))
                    ? xMLParser.GetNamedAttributeValueSafe("name")
                    : "";
                if (!itemName.Empty())
                {
                    while (xMLParser.Read() == Error.Ok && worldName.Empty()
                    || (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                    && xMLParser.GetNodeName().Equals("itemClass")))
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                        && xMLParser.GetNodeName().Equals("level")
                        && short.Parse(xMLParser.GetNodeData()) == itemLevel)
                        {
                            worldName = itemName;
                        }
                    }
                }
            }
            
            return worldName;
        }
        public static string GetItemMaterial(string worldName)
        {
            string material = "";
            // Not implemented
            return material;
        }
    }
}

