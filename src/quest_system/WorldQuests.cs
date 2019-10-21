using Godot;

namespace Game.Quests
{
    public class WorldQuests : Node
    {
        private Node availableQuests;
        private Node activeQuests;
        private Node completedQuests;
        private Node deliveredQuests;
        private Quest focusedQuest;

        public override void _Ready()
        {
            LoadQuests();
        }
        private void LoadQuests()
        {
            GD.Print("Not implemented");
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
        private void MoveQuest(Quest quest, Node to)
        {
            quest.GetParent().RemoveChild(quest);
            to.AddChild(quest);
            quest.ChangeState(to.GetName());
            if (to == deliveredQuests && quest.GetChildCount() > 0)
            {
                MoveQuest((Quest)quest.GetChild(0), availableQuests);
            }
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
            Quest linkedQuest = null;
            if (IsFocusedQuestChained())
            {
                linkedQuest = (Quest)focusedQuest.GetChild(0);
            }
            MoveQuest(focusedQuest, deliveredQuests);
            focusedQuest = linkedQuest;
        }
        public void Update()
        {
            foreach (Node questCatagory in GetChildren())
            {
                foreach (Quest quest in questCatagory.GetChildren())
                {
                    quest.ChangeState(questCatagory.GetName());
                }
            }
        }
        public Quest GetFocusedQuest()
        {
            return focusedQuest;
        }
    }
}