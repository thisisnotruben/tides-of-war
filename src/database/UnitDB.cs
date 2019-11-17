using Godot;
using System.Linq;
using System.Collections.Generic;

namespace Game.Database
{
    public static class UnitDB
    {
        private static readonly string DB_PATH = "res://src/database/data/UnitDB.xml";
        private static readonly string[] ignoreTags = { "items", "spells" };
        private static readonly string[] namedTags = { "item", "spell" };
        private static readonly XMLParser xMLParser = new XMLParser();

        public static Dictionary<string, string> GetUniqueUnitData(string unitEditorName, string mapName)
        {
            Dictionary<string, string> unitData = new Dictionary<string, string>();
            bool endLoop = false;
            short count = 0;
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && !endLoop)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals(mapName))
                {
                    while (xMLParser.Read() == Error.Ok && !endLoop)
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                        && xMLParser.GetNamedAttributeValueSafe("name").Equals(unitEditorName))
                        {
                            while (xMLParser.Read() == Error.Ok
                            && !(xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                            && xMLParser.GetNodeName().Equals("unit")))
                            {
                                string keyName = (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                                    ? keyName = xMLParser.GetNodeName() :
                                    "";
                                if (!ignoreTags.Contains(keyName))
                                {
                                    if (namedTags.Contains(keyName))
                                    {
                                        keyName = (unitData.ContainsKey(keyName)) ? keyName + count++.ToString() : keyName;
                                        unitData.Add(keyName, xMLParser.GetNamedAttributeValue("name"));
                                    }
                                    else if (!keyName.Empty() && xMLParser.Read() == Error.Ok
                                    && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                                    {
                                        unitData.Add(keyName, xMLParser.GetNodeData());
                                    }
                                }
                            }
                            endLoop = true;
                        }
                    }
                }
            }
            return unitData;
        }
        public static bool IsUnitUnique(string unitEditorName, string mapName)
        {
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals(mapName))
                {
                    while (xMLParser.Read() == Error.Ok)
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                        && xMLParser.GetNamedAttributeValueSafe("name").Equals(unitEditorName))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool IsUnitGeneric(string worldName)
        {
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals("generic"))
                {
                    while (xMLParser.Read() == Error.Ok)
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                        && xMLParser.GetNodeName().Equals("worldName")
                        && xMLParser.Read() == Error.Ok
                        && xMLParser.GetNodeType() == XMLParser.NodeType.Text
                        && xMLParser.GetNodeData().Equals(worldName))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool GetGenericUnitEnemy(string imgPath)
        {
            string[] splittedPath = imgPath.Split("/");
            string raceName = splittedPath[splittedPath.Length - 2];
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals("generic"))
                {
                    while (xMLParser.Read() == Error.Ok)
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                        && xMLParser.GetNodeName().Equals(raceName))
                        {
                            return bool.Parse(xMLParser.GetNamedAttributeValue("enemy"));
                        }
                    }
                }
            }
            return false;
        }
        public static string GetGenericUnitName(string imgPath)
        {
            string[] splittedPath = imgPath.BaseName().Split("/");
            string fileName = splittedPath[splittedPath.Length - 1];
            string genericUnitName = "";
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok && genericUnitName.Empty())
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals("generic"))
                {
                    while (xMLParser.Read() == Error.Ok && genericUnitName.Empty())
                    {
                        if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                        {
                            string sprite = (xMLParser.GetNodeName().Equals("sprite"))
                                ? xMLParser.GetNamedAttributeValueSafe("imgName")
                                : "";
                            if (sprite.Equals(fileName))
                            {
                                while (xMLParser.Read() == Error.Ok && genericUnitName.Empty())
                                {
                                    if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                                    && xMLParser.GetNodeName().Equals("worldName")
                                    && xMLParser.Read() == Error.Ok
                                    && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                                    {
                                        genericUnitName = xMLParser.GetNodeData();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return genericUnitName;
        }
    }
}