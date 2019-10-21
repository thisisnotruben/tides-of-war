using Godot;
using Game.Misc.Loot;
using Game.Actor;

namespace Game.Quests
{
    public class Quest : Node
    {
        private string questGiver;
        private Godot.Collections.Dictionary objective;
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

        public void SetQuestGiver(string questGiverPath)
        {
            questDelivered = "/root".PlusFile(questGiverPath);
        }
        public void SetQuestName(string questName)
        {
            this.questName = questName;
            SetName(questName);
            LoadText();
        }
        public string GetQuestName()
        {
            return questName;
        }
        private void LoadText()
        {
            XMLParser xMLParser = new XMLParser();
            if (xMLParser.Open(string.Format("res://meta/dialogue/{0}.xml", questName)) == Error.Ok)
            {
                string testString = "?";
                while (xMLParser.Read() == Error.Ok)
                {
                    if (xMLParser.GetNodeType() == XMLParser.NodeType.Element)
                    {
                        string varName = testString;
                        switch (xMLParser.GetNodeName())
                        {
                            case "start":
                                varName = questStart;
                                break;
                            case "active":
                                varName = questActive;
                                break;
                            case "completed":
                                varName = questCompleted;
                                break;
                            case "delivered":
                                varName = questDelivered;
                                break;
                            case "r_completed":
                                varName = questRecieverCompleted;
                                break;
                            case "r_delivered":
                                varName = questRecieverDelivered;
                                break;
                        }
                        if (!varName.Equals(testString) && xMLParser.Read() == Error.Ok)
                        {
                            string text = xMLParser.GetNodeData().
                                Replace("\n        ", " ").Replace("  ", " ").StripEdges();
                            Set(nameof(varName), text);
                        }
                    }
                }
            }
        }
        public string GetQuestStartText()
        {
            return questStart;
        }
        public void SetReward()
        {
            GD.Print("Not Implemented");
        }
        public Pickable GetReward()
        {
            return reward;
        }
        public void SetGold(short gold)
        {
            this.gold = gold;
        }
        public short GetGold()
        {
            return gold;
        }
        public bool IsPartOf(WorldObject worldObject)
        {
            return objective.ContainsKey(worldObject.GetWorldName());
        }
        public bool CheckQuest(WorldObject worldObject)
        {
            var keyContent = objective[worldObject.GetWorldName()];
            if (keyContent is Godot.Collections.Array)
            {
                Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContent;
                if (keyContentArray.Count == 3)
                {

                }
                else if ((short)keyContentArray[0] < (short)keyContentArray[1])
                {
                    // ((short)keyContentArray[0])++;
                    if (keyContentArray.Count == 3)
                    {
                        Npc npc = (Npc)worldObject;
                        npc.SetText(questRecieverCompleted);
                    }
                }
            }
            else if (keyContent is Godot.Collections.Dictionary)
            {

            }
            return IsCompleted();
        }
        public bool IsCompleted()
        {
            short questCompletionTracker = 0;
            foreach (string task in objective.Keys)
            {
                var keyContent = objective[task];
                if (keyContent is Godot.Collections.Array)
                {
                    Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContent;
                    if (keyContentArray[0] == keyContentArray[1])
                    {
                        questCompletionTracker++;
                    }
                }
                else if (keyContent is Godot.Collections.Dictionary)
                {
                    Godot.Collections.Dictionary keyContentDic = (Godot.Collections.Dictionary)keyContent;
                    if (keyContentDic.ContainsKey("amount"))
                    {
                        Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContentDic["amount"];
                        if (keyContentArray[0] == keyContentArray[1])
                        {
                            questCompletionTracker++;
                        }
                    }
                    else if (keyContentDic.ContainsKey("spoken"))
                    {
                        bool spoken = (bool)keyContentDic["spoken"];
                        if (spoken)
                        {
                            questCompletionTracker++;
                        }
                    }
                }
            }
            return questCompletionTracker == objective.Count;
        }
        public void ChangeState(string state)
        {
            if (HasNode(questGiver))
            {
                Npc npc = GetNode(questGiver) as Npc;
                if (npc != null)
                {
                    switch (state)
                    {
                        case "available":
                            npc.SetText(questStart);
                            break;
                        case "active":
                            npc.SetText(questActive);
                            break;
                        case "completed":
                            npc.SetText(questCompleted);
                            break;
                        case "delivered":
                            npc.SetText(questDelivered);
                            break;
                    }
                }
                else
                {
                    GD.Print("Unexpected type in method ChangeState");
                }
            }
            else
            {
                GD.Print("Map doesn't have: " + questGiver);
            }
        }
        public string GetState()
        {
            return GetParent().GetName();
        }
        public string FormatWithObjectiveText()
        {
            string format = "{0}: {1}/{2}\n";
            string text = "";
            foreach (string worldName in objective.Keys)
            {
                var keyContent = objective[worldName];
                if (keyContent is Godot.Collections.Array)
                {
                    Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContent;
                    text += string.Format(format, worldName, keyContentArray[0], keyContentArray[1]);
                }
                else if (keyContent is Godot.Collections.Dictionary)
                {
                    Godot.Collections.Dictionary keyContentDic = (Godot.Collections.Dictionary)keyContent;
                    if (keyContentDic.ContainsKey("amount"))
                    {
                        Godot.Collections.Array keyContentArray = (Godot.Collections.Array)keyContentDic["amount"];
                        text += string.Format(format, worldName, keyContentArray[0], keyContentArray[1]);
                    }
                    else if (keyContentDic.ContainsKey("spoken"))
                    {
                        bool spoken = (bool)keyContentDic["spoken"];
                        text += string.Format(format, worldName, (spoken) ? 1 : 0, 1);
                    }
                }
            }
            text = text.Remove(text.Length() - 2);
            text = "--Quest Objective--\n" + text;
            text = questStart.Insert(questStart.Find("--Rewards--"), text);
            return text;
        }
    }
}