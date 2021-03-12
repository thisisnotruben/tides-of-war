using Godot;
using Game.Database;
namespace Game.Ui
{
	public class PopupController : GameMenu
	{
		public Control yesNoView, errorView, exitView, filterView;
		public Label yesNoLabel, errorLabel;
		public Button yesBttn, noBttn, okayBttn, exitGameBttn,
			exitMenuBttn, allBttn, activeBttn, completedBttn;

		public override void _Ready()
		{
			base._Ready();

			Control popupContainer = GetChild<Control>(1);

			yesNoView = popupContainer.GetNode<Control>("yes_no");
			yesNoLabel = yesNoView.GetNode<Label>("label");
			yesBttn = yesNoView.GetNode<Button>("yes");
			noBttn = yesNoView.GetNode<Button>("no");

			errorView = popupContainer.GetNode<Control>("error");
			errorLabel = errorView.GetNode<Label>("label");
			okayBttn = errorView.GetNode<Button>("okay");

			exitView = popupContainer.GetNode<Control>("exit");
			exitGameBttn = exitView.GetNode<Button>("exit_game");
			exitMenuBttn = exitView.GetNode<Button>("exit_menu");

			filterView = popupContainer.GetNode<Control>("filter_options");
			allBttn = filterView.GetNode<Button>("all");
			activeBttn = filterView.GetNode<Button>("active");
			completedBttn = filterView.GetNode<Button>("completed");
		}
		public void OnResized() { GetChild<Control>(0).RectMinSize = GetChild<Control>(1).RectSize; }
		public void OnHide()
		{
			foreach (Control control in GetChild(1).GetChildren())
			{
				control.Hide();
			}
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