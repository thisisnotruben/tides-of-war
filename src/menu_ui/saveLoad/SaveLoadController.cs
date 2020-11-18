using Godot;
using Game.Database;
namespace Game.Ui
{
	public class SaveLoadController : GameMenu
	{
		private readonly SaveLoadModel saveLoadModel = new SaveLoadModel();
		private PopupController popupController;
		private int index;

		public override void _Ready()
		{
			AddChild(saveLoadModel);
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnSaveLoadNodeHide));
			popupController.GetNode<BaseButton>("m/save_load/save").Connect("pressed", this, nameof(_OnSavePressed));
			SetLabels();
		}
		private void SaveGame()
		{
			saveLoadModel.SaveGame(index);
			GetLabel(index - 1).Text = SaveLoadModel.GetSaveFileName(index);
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
					GetLabel(labelIdx - 1).Text = SaveLoadModel.GetSaveFileName(labelIdx);
					labelIdx++;
				}
			}
		}
		protected void RouteConnections(string toMethod)
		{
			BaseButton yesBttn = popupController.GetNode<BaseButton>("m/yes_no/yes");
			string signal = "pressed";
			foreach (Godot.Collections.Dictionary connectionPacket in yesBttn.GetSignalConnectionList(signal))
			{
				yesBttn.Disconnect(signal, this, (string)connectionPacket["method"]);
			}
			yesBttn.Connect(signal, this, toMethod);
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
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);
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
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
			RouteConnections(nameof(_OnDeleteConfirm));
			popupController.GetNode<Label>("m/yes_no/label").Text = "Delete?";
			popupController.GetNode<Control>("m/yes_no").Show();
			popupController.Show();
		}
		public void _OnDeleteConfirm()
		{
			SaveLoadModel.DeleteSaveFile(index);
			GetLabel(index).Text = SaveLoadModel.GetSaveFileName(index);
			_OnSaveLoadNodeHide();
		}
		public void _OnSavePressed()
		{
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK1);
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
			Globals.soundPlayer.PlaySound(NameDB.UI.CLICK0);
			saveLoadModel.LoadGame(index);
			Hide();
		}
	}
}
