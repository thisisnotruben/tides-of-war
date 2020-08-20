using System;
using System.Collections.Generic;
using Game.Actor;
using Game.Database;
using Game.Loot;
using Godot;
namespace Game.Quests
{
    public class WorldQuests : Node
    {
        public enum QuestState : byte { AVAILABLE, ACTIVE, COMPLETED, DELIVERED }
        private Node availableQuests;
        private Node activeQuests;
        private Node completedQuests;
        private Node deliveredQuests;
        private Quest focusedQuest;
        public override void _Ready()
        {
            availableQuests = GetNode(QuestState.AVAILABLE.ToString());
            activeQuests = GetNode(QuestState.ACTIVE.ToString());
            completedQuests = GetNode(QuestState.COMPLETED.ToString());
            deliveredQuests = GetNode(QuestState.DELIVERED.ToString());
            LoadQuests();
        }
        private void LoadQuests()
        {
            // TODO
            /*
            Dictionary<string, Dictionary<string, string>> allQuestData = QuestDB.GetAllQuestData();
            Dictionary<string, List<string>> chainQuestQueue = new Dictionary<string, List<string>>();
            PackedScene questScene = (PackedScene)GD.Load("res://src/quest_system/quest.tscn");
            foreach (string questName in allQuestData.Keys)
            {
                foreach (string key in allQuestData[questName].Keys)
                {
                    if (key.Contains("chainQuest"))
                    {
                        string chainQuestName = allQuestData[questName][key];
                        if (chainQuestQueue.ContainsKey(questName))
                        {
                            chainQuestQueue[questName].Add(chainQuestName);
                        }
                        else
                        {
                            chainQuestQueue.Add(questName, new List<string>() { chainQuestName });
                        }
                    }
                }
                Quest quest = (Quest)questScene.Instance();
                availableQuests.AddChild(quest);
                quest.SetData(allQuestData[questName]);
            }
            foreach (string rootQuestName in chainQuestQueue.Keys)
            {
                Quest rootQuest = (Quest)availableQuests.GetNode(rootQuestName);
                foreach (string questName in chainQuestQueue[rootQuestName])
                {
                    Quest linkedQuest = (Quest)availableQuests.GetNode(questName);
                    MoveQuest(linkedQuest, rootQuest);
                }
                rootQuest.ChangeState((QuestState)Enum.Parse(typeof(QuestState), availableQuests.Name));
            }
            */
        }
        public void Reset()
        {
            foreach (Node questCatagory in GetChildren())
            {
                foreach (Node quest in questCatagory.GetChildren())
                {
                    quest.QueueFree();
                }
            }
            LoadQuests();
        }
        public bool IsFocusedQuestChained()
        {
            return focusedQuest.GetChildCount() > 0;
        }
        public void StartFocusedQuest()
        {
            MoveQuest(focusedQuest, activeQuests);
        }
        public void FinishFocusedQuest()
        {
            Quest linkedQuest = (IsFocusedQuestChained()) ? (Quest)focusedQuest.GetChild(0) : null;
            MoveQuest(focusedQuest, deliveredQuests);
            focusedQuest = linkedQuest;
        }
        public void Update()
        {
            foreach (Node questCatagory in GetChildren())
            {
                foreach (Quest quest in questCatagory.GetChildren())
                {
                    quest.ChangeState((QuestState)Enum.Parse(typeof(QuestState), questCatagory.Name));
                }
            }
        }
        public void UpdateQuestPickable(Pickable pickable, bool add)
        {
            foreach (Node questCatagory in new Node[] { activeQuests, completedQuests })
            {
                if (questCatagory == completedQuests && add)
                {
                    break;
                }
                foreach (Quest quest in questCatagory.GetChildren())
                {
                    if (quest.IsPartOf(pickable))
                    {
                        if (quest.CheckQuest(pickable, add))
                        {
                            MoveQuest(quest, completedQuests);
                        }
                        else
                        {
                            MoveQuest(quest, activeQuests);
                        }
                    }
                }
            }
        }
        public void UpdateQuestCharacter(Character character)
        {
            foreach (Quest quest in activeQuests.GetChildren())
            {
                if (quest.IsPartOf(character) && quest.CheckQuest(character, true))
                {
                    MoveQuest(quest, completedQuests);
                }
            }
            foreach (Node questCatagory in new Node[] { availableQuests, completedQuests })
            {
                foreach (Quest quest in questCatagory.GetChildren())
                {
                    if (character.GetPath().Equals(quest.questGiver))
                    {
                        focusedQuest = quest;
                        // if (questCatagory == availableQuests)
                        // {
                        //     Globals.player.GetMenu().dialogue.GetNode<Control>("s/s/v/accept").Show();
                        // }
                        // else
                        // {
                        //     Globals.player.GetMenu().dialogue.GetNode<Control>("s/s/v/finish").Show();
                        // }
                        return;
                    }
                }
            }
        }
        private void MoveQuest(Quest quest, Node to)
        {
            quest.GetParent().RemoveChild(quest);
            to.AddChild(quest);
            QuestState state;
            if (Enum.TryParse(to.Name, out state))
            {
                quest.ChangeState(state);
            }
            if (to == deliveredQuests && quest.GetChildCount() > 0)
            {
                MoveQuest((Quest)quest.GetChild(0), availableQuests);
            }
        }
        public Quest GetFocusedQuest()
        {
            return focusedQuest;
        }
    }
}