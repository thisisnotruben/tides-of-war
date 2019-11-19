using System.Collections.Generic;
using Godot;
namespace Game.Database
{
    public static class SpellDB
    {
        private static readonly string DB_PATH = "res://src/database/data/SpellDB.xml";
        private static readonly XMLParser xMLParser = new XMLParser();
        public static Dictionary<string, string> GetSpellData(string worldName)
        {
            Dictionary<string, string> spellData = new Dictionary<string, string>();
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element &&
                    xMLParser.GetNodeName().Equals("spell") &&
                    xMLParser.GetNamedAttributeValueSafe("name").Equals(worldName))
                {
                    while (xMLParser.Read() == Error.Ok &&
                        !(xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd &&
                            xMLParser.GetNodeName().Equals("spell")))
                    {
                        string keyName = (xMLParser.GetNodeType() == XMLParser.NodeType.Element) ?
                            xMLParser.GetNodeName() :
                            "";
                        if (!keyName.Empty() && xMLParser.Read() == Error.Ok &&
                            xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                        {
                            spellData.Add(keyName, xMLParser.GetNodeData());
                        }
                    }
                }
            }
            return spellData;
        }
        public static bool HasSpell(string nameCheck)
        {
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element &&
                    xMLParser.GetNodeName().Equals("spell") &&
                    xMLParser.GetNamedAttributeValue("name").Equals(nameCheck))
                {
                    return true;
                }
            }
            return false;
        }
    }
}