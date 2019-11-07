using Godot;
using System.Linq;
using System.Collections.Generic;

namespace Game.Database
{
    public static class QuestDB
    {
        private static readonly string DB_PATH = "res://src/database/data/QuestDB.xml";
        private static readonly string[] namedTags = { "chainQuest", "pickable", "kill" };
        private static readonly string[] excludeNamedTags = { "chainQuests", "reward", "objective" };
        private static readonly XMLParser xMLParser = new XMLParser();

        public static Dictionary<string, Dictionary<string, string>> GetAllQuestData()
        {
            Dictionary<string, Dictionary<string, string>> allQuestData = new Dictionary<string, Dictionary<string, string>>();
            short count = 0;
            xMLParser.Open(DB_PATH);
            while (xMLParser.Read() == Error.Ok)
            {
                if (xMLParser.GetNodeType() == XMLParser.NodeType.Element
                && xMLParser.GetNodeName().Equals("quest"))
                {
                    string questName = xMLParser.GetNamedAttributeValue("name");
                    Dictionary<string, string> questdata = new Dictionary<string, string>()
                    {
                        {"questName", questName}
                    };
                    while (xMLParser.Read() == Error.Ok
                    && !(xMLParser.GetNodeType() == XMLParser.NodeType.ElementEnd
                    && xMLParser.GetNodeName().Equals("quest")))
                    {
                        string keyName = (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                            ? xMLParser.GetNodeName()
                            : "";
                        if (!excludeNamedTags.Contains(keyName))
                        {
                            if (namedTags.Contains(keyName))
                            {
                                questdata.Add(keyName + count++.ToString(), xMLParser.GetNamedAttributeValue("name"));
                            }
                            else if (!keyName.Empty() && xMLParser.Read() == Error.Ok
                            && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                            {
                                questdata.Add(keyName, xMLParser.GetNodeData());
                            }
                        }
                    }
                    allQuestData.Add(questName, questdata);
                    count = 0;
                }
            }
            return allQuestData;
        }
    }
}