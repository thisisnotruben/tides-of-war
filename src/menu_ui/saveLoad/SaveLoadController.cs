using Godot;
namespace Game.Ui
{
	public class SaveLoadController : GameMenu
	{
		private readonly Archiver archiver = new Archiver();
		private PopupController popupController;
		private int index;

		public override void _Ready()
		{
			AddChild(archiver);
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnSaveLoadNodeHide));
			popupController.GetNode<BaseButton>("m/save_load/save").Connect("pressed", this, nameof(_OnSavePressed));
			SetLabels();
		}
		private void SaveGame()
		{
			archiver.SaveGame(index);
			GetLabel(index - 1).Text = Archiver.GetSaveFileName(index);
		}
		private Label GetLabel(int index) { return GetNode<Label>($"v/s/c/g/slot_label_{index}"); }
		private void SetLabels()
		{

			int labelIdx = 1;
			foreach (Node node in GetNode<Node>("v/s/c/g").GetChildren())
			{
				Label label = node as Label;
				if (label != null)
				{
					GetLabel(labelIdx - 1).Text = Archiver.GetSaveFileName(labelIdx);
					labelIdx++;
				}
			}
		}
		public void RouteConnections(string toMethod)
		{
			// TODO: need to clear pervious signal list before connecting
			popupController.GetNode("m/yes_no/yes").Connect("pressed", this, toMethod);
		}
		public void _OnSaveLoadNodeHide()
		{
			popupController.Hide();
			GetNode<Control>("v").Show();
		}
		public void _OnPopupHide()
		{
			popupController.GetNode<Control>("m/save_load/delete").Hide();
			popupController.GetNode<Control>("m/save_load/save").Hide();
			popupController.GetNode<Control>("m/save_load/load").Hide();
		}
		public void _OnSlotPressed(int index)
		{
			this.index = index + 1;
			Globals.PlaySound("click2", this, speaker);
			GetNode<Control>("v").Hide();
			popupController.GetNode<Control>("m/save_load/save").Show();
			if (!GetLabel(index).Text.Equals($"Slot {index + 1}"))
			{
				popupController.GetNode<Control>("m/save_load/load").Show();
				popupController.GetNode<Control>("m/save_load/delete").Show();
			}
			popupController.GetNode<Control>("m/save_load").Show();
			popupController.Show();
		}
		public void _OnDeletePressed()
		{
			Globals.PlaySound("click1", this, speaker);
			RouteConnections(nameof(_OnDeleteConfirm));
			popupController.GetNode<Label>("m/yes_no/label").Text = "Delete?";
			popupController.GetNode<Control>("m/yes_no").Show();
			popupController.Show();
		}
		public void _OnDeleteConfirm()
		{
			Archiver.DeleteSaveFile(index);
			GetLabel(index).Text = Archiver.GetSaveFileName(index);
			_OnSaveLoadNodeHide();
		}
		public void _OnSavePressed()
		{
			Globals.PlaySound("click1", this, speaker);
			if (GetLabel(index).Text.Contains("Slot"))
			{
				_OnOverwriteConfirm();
			}
			else
			{
				RouteConnections(nameof(_OnOverwriteConfirm));
				popupController.GetNode<Label>("m/yes_no/label").Text = "Overwrite?";
				popupController.GetNode<Control>("m/yes_no").Show();
			}
		}
		public void _OnOverwriteConfirm()
		{
			_OnSaveLoadNodeHide();
			SaveGame();
		}
		public void _OnLoadPressed()
		{
			Globals.PlaySound("click0", this, speaker);
			archiver.LoadGame(index);
			Hide();
		}
	}
}
