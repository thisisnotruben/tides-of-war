using Godot;
using Game.Database;
namespace Game.Ui
{
	public class QuestLogController : GameMenu
	{
		private PopupController popupController;
		protected Control mainContent;

		public override void _Ready()
		{
			mainContent = GetNode<Control>("s");

			popupController = GetNode<PopupController>("popup");
			popupController.allBttn.Connect("pressed", this, nameof(_OnAllPressed));
			popupController.activeBttn.Connect("pressed", this, nameof(_OnActivePressed));
			popupController.completedBttn.Connect("pressed", this, nameof(_OnCompletedPressed));
			popupController.Connect("hide", this, nameof(_OnQuestLogNodeHide));
		}
		public void _OnQuestLogNodeHide()
		{
			mainContent.Show();
			popupController.Hide();
		}
		public void _OnFilterPressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);
			mainContent.Hide();
			popupController.filterView.Show();
			popupController.Show();
		}
		public void _OnAllPressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
			foreach (QuestEntryController questSlot in GetNode("s/v/s/quest_nodes").GetChildren())
			{
				questSlot.Show();
			}
			popupController.Hide();
			_OnQuestLogNodeHide();
		}
		public void _OnActivePressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
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
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
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