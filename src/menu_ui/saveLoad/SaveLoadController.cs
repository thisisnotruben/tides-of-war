using Godot;
using Game.Database;
namespace Game.Ui
{
	public class SaveLoadController : GameMenu
	{
		private readonly SaveLoadModel saveLoadModel = new SaveLoadModel();
		private Control main;
		private PopupController popup;
		private int index;

		public override void _Ready()
		{
			main = GetChild<Control>(0);

			popup = GetChild<PopupController>(1);
			popup.Connect("hide", this, nameof(OnHide));
			popup.saveBttn.Connect("pressed", this, nameof(OnSavePressed));

			AddChild(saveLoadModel);
			SetLabels();
		}
		private void SaveGame()
		{
			saveLoadModel.SaveGame(index);
			GetLabel(index - 1).Text = SaveLoadModel.GetSaveFileName(index);
		}
		private Label GetLabel(int index) { return main.GetChild(0).GetNode<Label>("slot_label_" + index); }
		private void SetLabels()
		{
			int labelIdx = 1;
			foreach (Node node in main.GetChild(0).GetChildren())
			{
				if (node as Label != null)
				{
					GetLabel(labelIdx - 1).Text = SaveLoadModel.GetSaveFileName(labelIdx);
					labelIdx++;
				}
			}
		}
		private void RouteConnection(string toMethod)
		{
			BaseButton yesBttn = popup.yesBttn;
			string signal = "pressed";
			foreach (Godot.Collections.Dictionary connectionPacket in yesBttn.GetSignalConnectionList(signal))
			{
				yesBttn.Disconnect(signal, this, (string)connectionPacket["method"]);
			}
			yesBttn.Connect(signal, this, toMethod);
		}
		private void OnHide()
		{
			popup.Hide();
			main.Show();
		}
		private void OnPopupHide() { popup.deleteBttn.Visible = popup.saveBttn.Visible = popup.loadBttn.Visible = false; }
		private void OnSlotPressed(int index)
		{
			this.index = index + 1;
			PlaySound(NameDB.UI.CLICK2);
			main.Hide();
			popup.loadBttn.Visible = popup.deleteBttn.Visible = !GetLabel(index).Text.Equals($"Slot {index + 1}");
			popup.Visible = popup.saveLoadView.Visible = popup.saveBttn.Visible = true;
		}
		private void OnDeletePressed()
		{
			PlaySound(NameDB.UI.CLICK1);
			RouteConnection(nameof(OnDeleteConfirm));
			popup.ShowConfirm("Delete?");
		}
		private void OnDeleteConfirm()
		{
			SaveLoadModel.DeleteSaveFile(index);
			GetLabel(index).Text = SaveLoadModel.GetSaveFileName(index);
			OnHide();
		}
		private void OnSavePressed()
		{
			PlaySound(NameDB.UI.CLICK1);
			if (GetLabel(index).Text.Contains("Slot"))
			{
				OnSaveConfirm();
			}
			else
			{
				RouteConnection(nameof(OnSaveConfirm));
				popup.ShowConfirm("Overwrite?");
			}
		}
		private void OnSaveConfirm()
		{
			SaveGame();
			OnHide();
		}
		private void OnLoadPressed()
		{
			PlaySound(NameDB.UI.CLICK0);
			saveLoadModel.LoadGame(index);
		}
	}
}