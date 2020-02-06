using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Game.Database
{
    public static class UnitDB
    {
        private static readonly string[] ignoreTags = { "items", "spells", "path" };
        private static readonly string[] namedTags = { "item", "spell" };
        private static readonly XMLParser xMLParser = new XMLParser();
        public static Dictionary<string, string> GetUnitData(string unitEditorName, string mapName)
        {
            Dictionary<string, string> unitData = new Dictionary<string, string>();
            short count = 0;
            xMLParser.Open($"res://data/{mapName}DB.xml");
            while (xMLParser.Read() == Error.Ok && unitData.Count == 0)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element &&
                    xMLParser.GetNamedAttributeValueSafe("editorName").Equals(unitEditorName))
                {
                    unitData.Add("img", xMLParser.GetNamedAttributeValueSafe("img"));
                    unitData.Add("name", xMLParser.GetNamedAttributeValueSafe("name"));
                    unitData.Add("enemy", xMLParser.GetNamedAttributeValueSafe("enemy"));
                    unitData.Add("spawnPos",
                        $"{xMLParser.GetNamedAttributeValueSafe("x")},{xMLParser.GetNamedAttributeValueSafe("y")}");
                    string path = "";
                    while (xMLParser.Read() == Error.Ok &&
                        !(xMLParser.GetNodeType() == XMLParser.NodeType.Element &&
                            xMLParser.GetNodeName().Equals("unit")))
                    {
                        string keyName = (xMLParser.GetNodeType() == XMLParser.NodeType.Element) ?
                            keyName = xMLParser.GetNodeName() :
                            "";
                        if (!ignoreTags.Contains(keyName))
                        {
                            if (keyName.Equals("point"))
                            {
                                path += $"{xMLParser.GetNamedAttributeValueSafe("x")},{xMLParser.GetNamedAttributeValueSafe("y")}_";
                            }
                            else if (namedTags.Contains(keyName))
                            {
                                unitData.Add(keyName + count++.ToString(), xMLParser.GetNamedAttributeValue("name"));
                            }
                            else if (!keyName.Empty() && xMLParser.Read() == Error.Ok &&
                                xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                            {
                                unitData.Add(keyName, xMLParser.GetNodeData());
                            }
                        }
                    }
                    if (!path.Empty())
                    {
                        unitData.Add("path", path);
                    }
                }
            }
            return unitData;
        }
    }
}