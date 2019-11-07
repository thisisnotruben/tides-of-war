using Godot;
using Game.Misc.Loot;
using Game.Spell;
using Game.Actor;
using Game.Database;
using System.Collections.Generic;

namespace Game.Quests
{
    public class Quest : Node
    {
        private string questGiver;
        private string questName;
        private string questStart;
        private string questActive;
        private string questCompleted;
        private string questDelivered;
        private string questRecieverCompleted;
        private string questRecieverDelivered;
        private bool keepPickables;
        private short gold;
        private Pickable reward;
        private Dictionary<string, string> objective = new Dictionary<string, string>();
        private WorldQuests.QuestState state;

        public string GetQuestGiverPath()
        {
            return questGiver;
        }
        public string GetQuestName()
        {
            return questName;
        }
        public short GetGold()
        {
            return gold;
        }
        public string GetQuestStartText()
        {
            return questStart;
        }
        public Pickable GetReward()
        {
            return reward;
        }
        public WorldQuests.QuestState GetState()
        {
            return state;
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
                GD.Print("Map doesn't have: " + questGiver);
            }
        }
        public bool IsPartOf(WorldObject worldObject)
        {
            return objective.ContainsKey(worldObject.GetWorldName());
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
                    if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
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
                            GD.Print($"Invalid dialogue tag in Quest: ({questName}). Tag: {tagName}");
                        }
                        else if (xMLParser.Read() == Error.Ok
                        && xMLParser.GetNodeType() == XMLParser.NodeType.Text)
                        {
                            string text = xMLParser.GetNodeData();
                            text = text.Replace("\n        ", " ").Replace("  ", " ").StripEdges();
                            Set(varName, text);
                        }
                        else
                        {
                            GD.Print($"Unexpected error in quest: ({questName}) in reading tags.");
                        }
                    }
                }
            }
            else
            {
                GD.Print($"Quest: ({questName}) doesn't have dialogue file.");
            }
        }
        public bool CheckQuest(WorldObject worldObject, bool add)
        {
            // var keyContent = objective[worldObject.GetWorldName()];
            // if (keyContent is Godot.Collections.Array)
            // {
            //     Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContent;
            //     if (keyContentArray.Count == 3)
            //     {

            //     }
            //     else if ((short)keyContentArray[0] < (short)keyContentArray[1])
            //     {
            //         // ((short)keyContentArray[0])++;
            //         if (keyContentArray.Count == 3)
            //         {
            //             Npc npc = (Npc)worldObject;
            //             npc.SetText(questRecieverCompleted);
            //         }
            //     }
            // }
            // else if (keyContent is Godot.Collections.Dictionary)
            // {

            // }
            return IsCompleted();
        }
        public bool IsCompleted()
        {
            short questCompletionTracker = 0;
            // foreach (string task in objective.Keys)
            // {
            //     var keyContent = objective[task];
            //     if (keyContent is Godot.Collections.Array)
            //     {
            //         Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContent;
            //         if (keyContentArray[0] == keyContentArray[1])
            //         {
            //             questCompletionTracker++;
            //         }
            //     }
            //     else if (keyContent is Godot.Collections.Dictionary)
            //     {
            //         Godot.Collections.Dictionary keyContentDic = (Godot.Collections.Dictionary)keyContent;
            //         if (keyContentDic.ContainsKey("amount"))
            //         {
            //             Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContentDic["amount"];
            //             if (keyContentArray[0] == keyContentArray[1])
            //             {
            //                 questCompletionTracker++;
            //             }
            //         }
            //         else if (keyContentDic.ContainsKey("spoken"))
            //         {
            //             bool spoken = (bool)keyContentDic["spoken"];
            //             if (spoken)
            //             {
            //                 questCompletionTracker++;
            //             }
            //         }
            //     }
            // }
            return questCompletionTracker == objective.Count;
        }
        public string FormatWithObjectiveText()
        {
            // string format = "{0}: {1}/{2}\n";
            string text = "";
            // foreach (string worldName in objective.Keys)
            // {
            //     var keyContent = objective[worldName];
            //     if (keyContent is Godot.Collections.Array)
            //     {
            //         Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContent;
            //         text += string.Format(format, worldName, keyContentArray[0], keyContentArray[1]);
            //     }
            //     else if (keyContent is Godot.Collections.Dictionary)
            //     {
            //         Godot.Collections.Dictionary keyContentDic = (Godot.Collections.Dictionary)keyContent;
            //         if (keyContentDic.ContainsKey("amount"))
            //         {
            //             Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContentDic["amount"];
            //             text += string.Format(format, worldName, keyContentArray[0], keyContentArray[1]);
            //         }
            //         else if (keyContentDic.ContainsKey("spoken"))
            //         {
            //             bool spoken = (bool)keyContentDic["spoken"];
            //             text += string.Format(format, worldName, (spoken) ? 1 : 0, 1);
            //         }
            //     }
            // }
            // text = text.Remove(text.Length() - 2);
            // text = "--Quest Objective--\n" + text;
            // text = questStart.Insert(questStart.Find("--Rewards--"), text);
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
                        SetName(questName);
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
                            GD.Print($"Quest: {questName} has invalid reward name: {data[key]}");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}