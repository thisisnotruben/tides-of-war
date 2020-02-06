using System;
using System.Collections.Generic;
using Game.Ability;
using Game.Actor;
using Game.Database;
using Game.Loot;
using Godot;
namespace Game.Quests
{
    public class Quest : Node
    {
        public string questGiver { get; private set; }
        public string questName { get; private set; }
        public string questStart { get; private set; }
        private string questActive;
        private string questCompleted;
        private string questDelivered;
        private string questRecieverCompleted;
        private string questRecieverDelivered;
        private bool keepPickables;
        private short gold;
        public Pickable reward { get; private set; }
        private Dictionary<string, string> objective = new Dictionary<string, string>();
        public WorldQuests.QuestState state { get; private set; }

        public short GetGold()
        {
            return gold;
        }
        public void ChangeState(WorldQuests.QuestState state)
        {
            if (HasNode(questGiver))
            {
                Npc npc = (Npc)GetNode(questGiver);
                switch (state)
                {
                    case WorldQuests.QuestState.AVAILABLE:
                        npc.SetText(questStart);
                        break;
                    case WorldQuests.QuestState.ACTIVE:
                        npc.SetText(questActive);
                        break;
                    case WorldQuests.QuestState.COMPLETED:
                        npc.SetText(questCompleted);
                        break;
                    case WorldQuests.QuestState.DELIVERED:
                        npc.SetText(questDelivered);
                        break;
                }
            }
            else
            {
                GD.Print($"Quest: ({questName}). Map doesn't have: ({questGiver})");
            }
        }
        public bool IsPartOf(WorldObject worldObject)
        {
            return objective.ContainsKey(worldObject.worldName);
        }
        private void LoadText()
        {
            XMLParser xMLParser = new XMLParser();
            string filePath = $"res://meta/dialogue/{questName}.xml";
            if (new File().FileExists(filePath) && xMLParser.Open(filePath) == Error.Ok)
            {
                string testString = "?";
                while (xMLParser.Read() == Error.Ok)
                {
                    if (xMLParser.GetNodeType() == XMLParser.NodeType.Element && !xMLParser.GetNodeName().Equals("dialogue"))
                    {
                        string varName = testString;
                        string tagName = xMLParser.GetNodeName();
                        switch (tagName)
                        {
                            case "start":
                                varName = nameof(questStart);
                                break;
                            case "active":
                                varName = nameof(questActive);
                                break;
                            case "completed":
                                varName = nameof(questCompleted);
                                break;
                            case "delivered":
                                varName = nameof(questDelivered);
                                break;
                            case "r_completed":
                                varName = nameof(questRecieverCompleted);
                                break;
                            case "r_delivered":
                                varName = nameof(questRecieverDelivered);
                                break;
                        }
                        if (varName.Equals(testString))
                        {
                            GD.Print($"Quest: ({questName}). Invalid dialogue tag: {tagName}");
                        }
                        else if (xMLParser.Read() == Error.Ok &&
                            xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                        {
                            string text = xMLParser.GetNodeData();
                            text = text.Replace("\n        ", " ").Replace("  ", " ").StripEdges();
                            Set(varName, text);
                        }
                        else
                        {
                            GD.Print($"Quest: ({questName}). Unexpected error in reading tags");
                        }
                    }
                }
            }
            else
            {
                GD.Print($"Quest: ({questName}). Doesn't have dialogue file");
            }
        }
        public bool CheckQuest(WorldObject worldObject, bool add)
        {
            foreach (string key in objective.Keys)
            {
                if (key.Equals(worldObject.worldName))
                {
                    string[] objectiveValues = objective[key].Split("-");
                    byte tracker = byte.Parse(objectiveValues[1]);
                    byte completion = byte.Parse(objectiveValues[2]);
                    if (tracker < completion)
                    {
                        tracker++;
                    }
                    objective[key] = $"{objectiveValues[0]}-{tracker}-{completion}";
                }
            }
            return IsCompleted();
        }
        public bool IsCompleted()
        {
            short questCompletionTracker = 0;
            foreach (string key in objective.Keys)
            {
                string[] objectiveValues = objective[key].Split("-");
                byte tracker = byte.Parse(objectiveValues[1]);
                byte completion = byte.Parse(objectiveValues[2]);
                if (tracker == completion)
                {
                    questCompletionTracker++;
                }
            }
            return questCompletionTracker == objective.Count;
        }
        public string FormatWithObjectiveText()
        {
            string text = "--Quest Objective--\n";
            foreach (string key in objective.Keys)
            {
                string[] objectiveValues = objective[key].Split("-");
                text += $"{key}: {objectiveValues[1]}/{objectiveValues[2]}\n";
            }
            text = text.Remove(text.Length() - 2); // remove the last "\n" character
            text = questStart.Insert(questStart.Find("--Rewards--"), text);
            return text;
        }
        public void SetData(Dictionary<string, string> data)
        {
            foreach (string key in data.Keys)
            {
                switch (key)
                {
                    case nameof(questGiver):
                        questGiver = "/root".PlusFile(data[key]);
                        break;
                    case nameof(questName):
                        questName = data[key];
                        Name = questName;
                        LoadText();
                        break;
                    case nameof(gold):
                        gold = short.Parse(data[key]);
                        break;
                    case nameof(keepPickables):
                        keepPickables = bool.Parse(data[key]);
                        break;
                    case "pickable":
                        if (SpellDB.HasSpell(data[key]))
                        {
                            reward = PickableFactory.GetMakeSpell(data[key]);
                        }
                        else if (ItemDB.HasItem(data[key]))
                        {
                            reward = PickableFactory.GetMakeItem(data[key]);
                        }
                        else
                        {
                            GD.Print($"Quest: ({questName}), has invalid reward name: {data[key]}");
                        }
                        break;
                    default:
                        objective.Add(key, data[key]);
                        break;
                }
            }
        }
    }
}