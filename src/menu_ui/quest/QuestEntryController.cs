using Godot;
using Game.Quest;
namespace Game.Ui
{
	public class QuestEntryController : Control
	{
		private Label label;
		public BaseButton button { get; private set; }

		public WorldQuest worldQuest;

		public override void _Ready()
		{
			label = GetNode<Label>("colorRect/marginContainer/label");
			button = GetNode<BaseButton>("colorRect/marginContainer/label/button");
		}
		public QuestEntryController Init(WorldQuest worldQuest)
		{
			this.worldQuest = worldQuest;
			return this;
		}
		public void Display(string text) { label.Text = text; }
	}
}