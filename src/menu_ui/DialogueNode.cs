using Godot;
using System;
using Game.Utils;
namespace Game.Ui
{
    public class DialogueNode : Control
    {
        public Speaker speaker;

        public void Display(string header, string body)
        {
            GetNode<Label>("s/header").Text = header;
            GetNode<RichTextLabel>("s/s/text").BbcodeText = body;
        }
        public void _OnAcceptPressed()
        {
            // TODO: quest code
            // Globals.PlaySound("quest_accept", this, speaker);
            // Globals.worldQuests.StartFocusedQuest();
            // QuestEntry questEntry = (QuestEntry)questEntryScene.Instance();
            // questEntry.AddToQuestLog(questLog);
            // questEntry.quest = Globals.worldQuests.GetFocusedQuest();
            // GetNode<Control>("s/s/v/accept").Hide();
            // GetNode<Control>("s/s/v/finish").Hide();
            // Hide();
        }
        public void _OnFinishPressed()
        {
            // TODO: quest code
            // Quest quest = Globals.worldQuests.GetFocusedQuest();
            // if (quest.GetGold() > 0)
            // {
            //     Globals.PlaySound("sell_buy", this, snd);
            //     player.gold = quest.GetGold();
            //     CombatText combatText = (CombatText)Globals.combatText.Instance();
            //     player.AddChild(combatText);
            //     combatText.SetType($"+{quest.GetGold().ToString("N0")}",
            //         CombatText.TextType.GOLD, player.GetNode<Node2D>("img").Position);
            // }
            // Pickable questReward = quest.reward;
            // if (questReward != null)
            // {
            //     Globals.map.AddZChild(questReward);
            //     questReward.GlobalPosition = Globals.map.GetGridPosition(player.GlobalPosition);
            // }
            // Globals.worldQuests.FinishFocusedQuest();
            // if (Globals.worldQuests.GetFocusedQuest() != null)
            // {
            //     dialogue.GetNode<Label>("s/s/label2").Text = Globals.worldQuests.GetFocusedQuest().questStart;
            // }
            // else
            // {
            //     _OnBackPressed();
            // }
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}