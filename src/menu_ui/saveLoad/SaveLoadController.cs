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
			popupController.saveBttn.Connect("pressed", this, nameof(_OnSavePressed));
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
			BaseButton yesBttn = popupController.yesBttn;
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
			popupController.deleteBttn.Hide();
			popupController.saveBttn.Hide();
			popupController.loadBttn.Hide();
		}
		public void _OnSlotPressed(int index)
		{
			this.index = index + 1;
			PlaySound(NameDB.UI.CLICK2);
			GetNode<Control>("v").Hide();
			popupController.saveBttn.Show();
			if (!GetLabel(index).Text.Equals($"Slot {index + 1}"))
			{
				popupController.loadBttn.Show();
				popupController.deleteBttn.Show();
			}
			popupController.saveLoadView.Show();
			popupController.Show();
		}
		public void _OnDeletePressed()
		{
			PlaySound(NameDB.UI.CLICK1);
			RouteConnections(nameof(_OnDeleteConfirm));
			popupController.ShowConfirm("Delete?");
		}
		public void _OnDeleteConfirm()
		{
			SaveLoadModel.DeleteSaveFile(index);
			GetLabel(index).Text = SaveLoadModel.GetSaveFileName(index);
			_OnSaveLoadNodeHide();
		}
		public void _OnSavePressed()
		{
			PlaySound(NameDB.UI.CLICK1);
			if (GetLabel(index).Text.Contains("Slot"))
			{
				_OnOverwriteConfirm();
			}
			else
			{
				RouteConnections(nameof(_OnOverwriteConfirm));
				popupController.ShowConfirm("Overwrite?");
			}
		}
		public void _OnOverwriteConfirm()
		{
			_OnSaveLoadNodeHide();
			SaveGame();
		}
		public void _OnLoadPressed()
		{
			PlaySound(NameDB.UI.CLICK0);
			saveLoadModel.LoadGame(index);
			Hide();
		}
	}
}