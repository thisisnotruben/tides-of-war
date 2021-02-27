using Godot;
using Game.Database;
using System.Collections.Generic;
namespace Game.Ui
{
	public class PopupController : GameMenu
	{
		public Control saveLoadView, yesNoView, errorView, exitView, filterView,
			addToSlotView, repairSlotView;
		public Label yesNoLabel, errorLabel;
		public Button deleteBttn, saveBttn, loadBttn, yesBttn, noBttn, okayBttn,
			exitGameBttn, exitMenuBttn, allBttn, activeBttn, completedBttn, clearSlot, addToSlotBackBttn,
			repairBackBttn;
		public readonly List<Button> addToHudSlotBttns = new List<Button>();

		public override void _Ready()
		{
			base._Ready();

			Control popupContainer = GetChild<Control>(1);

			saveLoadView = popupContainer.GetNode<Control>("save_load");
			deleteBttn = saveLoadView.GetNode<Button>("delete");
			saveBttn = saveLoadView.GetNode<Button>("save");
			loadBttn = saveLoadView.GetNode<Button>("load");

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

			addToSlotView = popupContainer.GetNode<Control>("add_to_slot");
			Button button;
			foreach (Control control in addToSlotView.GetChildren())
			{
				button = control as Button;
				if (button != null && button.Name.Contains("slot_"))
				{
					addToHudSlotBttns.Add(button);
				}
			}
			clearSlot = addToSlotView.GetNode<Button>("clear_slot");
			addToSlotBackBttn = addToSlotView.GetNode<Button>("back");

			filterView = popupContainer.GetNode<Control>("filter_options");
			allBttn = filterView.GetNode<Button>("all");
			activeBttn = filterView.GetNode<Button>("active");
			completedBttn = filterView.GetNode<Button>("completed");

			repairSlotView = popupContainer.GetNode<Control>("repair");
			repairBackBttn = repairSlotView.GetNode<Button>("back");
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