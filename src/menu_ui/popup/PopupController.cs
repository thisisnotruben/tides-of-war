using Godot;
using Game.Database;
namespace Game.Ui
{
	public class PopupController : GameMenu
	{
		private Control yesNoView, errorView, filterView;
		public Label yesNoLabel, errorLabel;
		private Button yesBttn, okayBttn;

		public override void _Ready()
		{
			base._Ready();

			Control popupContainer = GetChild<Control>(1);

			yesNoView = popupContainer.GetNode<Control>("yes_no");
			yesNoLabel = yesNoView.GetNode<Label>("label");
			yesBttn = yesNoView.GetNode<Button>("yes");

			errorView = popupContainer.GetNode<Control>("error");
			errorLabel = errorView.GetNode<Label>("label");
			okayBttn = errorView.GetNode<Button>("okay");
		}
		public void OnResized() { GetChild<Control>(0).RectMinSize = GetChild<Control>(1).RectSize; }
		public void OnHide()
		{
			foreach (Control control in GetChild(1).GetChildren())
			{
				control.Hide();
			}
		}
		public void RouteConnection(string toMethod, Node target)
		{
			string signal = "pressed";
			foreach (Godot.Collections.Dictionary connectionPacket in yesBttn.GetSignalConnectionList(signal))
			{
				yesBttn.Disconnect(signal, target, connectionPacket["method"].ToString());
			}
			yesBttn.Connect(signal, target, toMethod);
		}
		public void ShowError(string errorText)
		{
			PlaySound(NameDB.UI.CLICK6);
			errorLabel.Text = errorText;
			Visible = errorView.Visible = true;
		}
		public void ShowConfirm(string confirmText)
		{
			yesNoLabel.Text = confirmText;
			Visible = yesNoView.Visible = true;
		}
	}
}