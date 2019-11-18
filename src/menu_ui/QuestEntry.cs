using Game.Quests;
using Godot;
namespace Game.Ui
{
    public class QuestEntry : Control
    {
        private Quest quest;
        public void AddToQuestLog(Node questLog)
        {
            questLog.GetNode(@"s/v/s/v").AddChild(this);
            questLog.GetNode(@"s/v/s/v").MoveChild(this, 0);
        }
        public void SetQuest(Quest quest)
        {
            GetNode<Label>("label").SetText(quest.GetQuestName());
            SetName(quest.GetQuestName());
            this.quest = quest;
        }
        public Quest GetQuest()
        {
            return quest;
        }
        public void _OnQuestSlotPressed()
        {
            Globals.player.GetMenu().ShowQuestText(quest);
        }
    }
}