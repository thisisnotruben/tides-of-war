using Godot;
namespace Game.Ui
{
	public class QuestLogController : GameMenu
	{
		private PopupController popupController;

		public override void _Ready()
		{
			popupController = GetNode<PopupController>("popup");
			popupController.GetNode<BaseButton>("m/filter_options/all")
				.Connect("pressed", this, nameof(_OnAllPressed));
			popupController.GetNode<BaseButton>("m/filter_options/active")
				.Connect("pressed", this, nameof(_OnActivePressed));
			popupController.GetNode<BaseButton>("m/filter_options/completed")
				.Connect("pressed", this, nameof(_OnCompletedPressed));
			popupController.Connect("hide", this, nameof(_OnQuestLogNodeHide));
		}
		public void _OnQuestLogNodeHide()
		{
			GetNode<Control>("s").Show();
			popupController.Hide();
		}
		public void _OnFilterPressed()
		{
			Globals.PlaySound("click2", this, speaker);
			GetNode<Control>("s").Hide();
			popupController.GetNode<Control>("m/filter_options").Show();
			popupController.Show();
		}
		public void _OnAllPressed()
		{
			Globals.PlaySound("click1", this, speaker);
			foreach (QuestEntryController questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				questSlot.Show();
			}
			popupController.Hide();
			_OnQuestLogNodeHide();
		}
		public void _OnActivePressed()
		{
			Globals.PlaySound("click1", this, speaker);
			foreach (QuestEntryController questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				// TODO
				// if (questSlot.quest.state != Game.Quests.WorldQuests.QuestState.ACTIVE)
				// {
				// questSlot.Hide();
				// }
				// else
				// {
				questSlot.Show();
				// }
			}
			_OnQuestLogNodeHide();
		}
		public void _OnCompletedPressed()
		{
			Globals.PlaySound("click1", this, speaker);
			foreach (QuestEntryController questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				// TODO
				// if (questSlot.quest.state != Game.Quests.WorldQuests.QuestState.DELIVERED)
				// {
				// 	questSlot.Hide();
				// }
				// else
				// {
				questSlot.Show();
				// }
			}
			_OnQuestLogNodeHide();
		}
	}
}