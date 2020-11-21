using Godot;
using Game.Database;
using System.Collections.Generic;
namespace Game.Ui
{
	public class PopupController : GameMenu
	{
		public static MainMenuController mainMenuController;

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

			saveLoadView = GetNode<Control>("m/save_load");
			deleteBttn = saveLoadView.GetNode<Button>("delete");
			saveBttn = saveLoadView.GetNode<Button>("save");
			loadBttn = saveLoadView.GetNode<Button>("load");

			yesNoView = GetNode<Control>("m/yes_no");
			yesBttn = yesNoView.GetNode<Button>("yes");
			noBttn = yesNoView.GetNode<Button>("no");

			errorView = GetNode<Control>("m/error");
			errorLabel = errorView.GetNode<Label>("label");
			okayBttn = errorView.GetNode<Button>("okay");

			exitView = GetNode<Control>("m/exit");
			exitGameBttn = exitView.GetNode<Button>("exit_game");
			exitMenuBttn = exitView.GetNode<Button>("exit_menu");

			addToSlotView = GetNode<Control>("m/add_to_slot");
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

			filterView = GetNode<Control>("m/filter_options");
			allBttn = filterView.GetNode<Button>("all");
			activeBttn = filterView.GetNode<Button>("active");
			completedBttn = filterView.GetNode<Button>("completed");

			repairSlotView = GetNode<Control>("m/repair");
			repairBackBttn = repairSlotView.GetNode<Button>("back");
		}
		public void _OnPopupDraw() { mainMenuController?.ShowBackground(false); }
		public void _OnPopupHide()
		{
			mainMenuController?.ShowBackground(true);
			foreach (Control control in GetNode("m").GetChildren())
			{
				control.Hide();
			}
		}
		public void _OnErrorDraw() { Globals.soundPlayer.PlaySound(NameDB.UI.CLICK6); }
		public void _OnMResized() { GetNode<Control>("bg").RectMinSize = GetNode<Control>("m").RectSize; }
		public void _OnRepairDraw()
		{
			int shown = 0;
			foreach (Control node in GetNode("m/repair").GetChildren())
			{
				if (node.Visible)
				{
					shown++;
				}
			}
			if (shown > 4)
			{
				GetNode<TextureRect>("bg").Texture = (Texture)GD.Load("res://asset/img/ui/grey2_bg.tres");
			}
		}
		public void _OnRepairHide() { GetNode<TextureRect>("bg").Texture = (Texture)GD.Load("res://asset/img/ui/grey3_bg.tres"); }
	}
}