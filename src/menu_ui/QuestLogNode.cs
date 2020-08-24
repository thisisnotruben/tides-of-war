using Godot;
namespace Game.Ui
{
	public class QuestLogNode : GameMenu
	{
		private Popup popup;

		public override void _Ready()
		{
			popup = GetNode<Popup>("popup");
			popup.GetNode<BaseButton>("m/filter_options/all")
				.Connect("pressed", this, nameof(_OnAllPressed));
			popup.GetNode<BaseButton>("m/filter_options/active")
				.Connect("pressed", this, nameof(_OnActivePressed));
			popup.GetNode<BaseButton>("m/filter_options/completed")
				.Connect("pressed", this, nameof(_OnCompletedPressed));
			popup.Connect("hide", this, nameof(_OnQuestLogNodeHide));
		}
		public void _OnQuestLogNodeHide()
		{
			GetNode<Control>("s").Show();
			popup.Hide();
		}
		public void _OnFilterPressed()
		{
			Globals.PlaySound("click2", this, speaker);
			GetNode<Control>("s").Hide();
			popup.GetNode<Control>("m/filter_options").Show();
			popup.Show();
		}
		public void _OnAllPressed()
		{
			Globals.PlaySound("click1", this, speaker);
			foreach (QuestEntry questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				questSlot.Show();
			}
			popup.Hide();
			_OnQuestLogNodeHide();
		}
		public void _OnActivePressed()
		{
			Globals.PlaySound("click1", this, speaker);
			foreach (QuestEntry questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				if (questSlot.quest.state != Game.Quests.WorldQuests.QuestState.ACTIVE)
				{
					questSlot.Hide();
				}
				else
				{
					questSlot.Show();
				}
			}
			_OnQuestLogNodeHide();
		}
		public void _OnCompletedPressed()
		{
			Globals.PlaySound("click1", this, speaker);
			foreach (QuestEntry questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				if (questSlot.quest.state != Game.Quests.WorldQuests.QuestState.DELIVERED)
				{
					questSlot.Hide();
				}
				else
				{
					questSlot.Show();
				}
			}
			_OnQuestLogNodeHide();
		}
	}
}