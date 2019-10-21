using Godot;
using System;

namespace Game.Database
{
    public class ItemDB : Node
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
            if (xMLParser.Open(DB_PATH) == Error.Ok)
            {
                while (xMLParser.Read() == Error.Ok && texture == null)
                {
                    if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                    {
                        string itemClass = (xMLParser.GetNodeName().Equals("itemClass"))
                            ? xMLParser.GetNamedAttributeValueSafe("name").ToLower()
                            : "";
                        if (xMLParser.GetNamedAttributeValueSafe("name").Equals(worldName))
                        {
                            string iconID = "";
                            while (xMLParser.Read() == Error.Ok && iconID.Empty()
                            || (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                            && xMLParser.GetNodeName().Equals("itemClass")))
                            {
                                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                                {
                                    iconID = xMLParser.GetNodeData();
                                }
                            }
                            texture = (AtlasTexture)GD.Load(string.Format("res://asset/img/icon/{0}/{1}_icon.res", itemClass, iconID));
                        }
                    }
                }
            }
            return texture;
        }
        public AtlasTexture GetItemIcon(potionClasses potionClasses, potionClasses potionType)
        {
            AtlasTexture texture = null;
            if (xMLParser.Open(DB_PATH) == Error.Ok)
            {
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
                            // if ()
                            // {
                            //     texture;
                            // }
                        }
                    }
                }
            }
            return texture;
        }
        public static int GetItemIconID(string worldName)
        {
            AtlasTexture texture = GetItemIcon(worldName);
            return Int32.Parse(texture.GetPath().GetFile().Split('_')[0]);
        }
        public static short GetItemLevel(string worldName)
        {
            short itemLevel = -1;
            if (xMLParser.Open(DB_PATH) == Error.Ok)
            {
                while (xMLParser.Read() == Error.Ok && itemLevel == -1)
                {
                    if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                    && xMLParser.GetNamedAttributeValueSafe("name").Equals(worldName))
                    {
                        while (xMLParser.Read() == Error.Ok && itemLevel == -1
                        || (xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                        && xMLParser.GetNodeName().Equals("itemClass")))
                        {
                            itemLevel = Int16.Parse(xMLParser.GetNodeData());
                        }
                    }
                }
            }
            return itemLevel;
        }
        public static string GetItemName(WorldObject.WorldTypes type, WorldObject.WorldTypes subType)
        {   
            GD.Print("Not implemented");
            return "";
        }
        public static string GetItemName(int iconID)
        {
            string worldName = "";
            if (xMLParser.Open(DB_PATH) == Error.Ok)
            {
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
                            && Int32.Parse(xMLParser.GetNodeData()) == iconID)
                            {
                                worldName = itemName;
                            }
                        }
                    }
                }
            }
            return worldName;
        }
        public string GetItemName(short itemLevel)
        // Not really a good function, the iconID works better
        {
            string worldName = "";
            if (xMLParser.Open(DB_PATH) == Error.Ok)
            {
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
                            && Int16.Parse(xMLParser.GetNodeData()) == itemLevel)
                            {
                                worldName = itemName;
                            }
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

