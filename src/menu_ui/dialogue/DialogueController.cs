using Godot;
using Game.Actor;
using Game.Quest;
namespace Game.Ui
{
	public class DialogueController : GameMenu
	{
		private PopupController popupController;
		private Label header, subHeader;
		private RichTextLabel richTextLabel;
		private Control mainContent, heal;
		public Button closeButton;

		private WorldQuest worldQuest;
		private Npc _npc;
		public Npc npc
		{
			get { return _npc; }
			set
			{
				_npc = value;
				Display(value);
			}
		}

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);
			header = mainContent.GetNode<Label>("vBoxContainer/header");
			subHeader = mainContent.GetNode<Label>("vBoxContainer/subHeader");
			richTextLabel = mainContent.GetNode<RichTextLabel>("vBoxContainer/text");
			heal = mainContent.GetNode<Control>("hBoxContainer/heal");
			closeButton = mainContent.GetNode<Button>("hBoxContainer/close");

			popupController = GetChild<PopupController>(1);
			popupController.Connect("hide", this, nameof(OnHide));
		}
		private void Display(Npc npc) { }
		private void OnHide()
		{
			popupController.Hide();
			mainContent.Show();
		}
	}
}