using Godot;
using Game.Database;
namespace Game.Ui
{
	public class SaveLoadController : GameMenu
	{
		public readonly SaveLoadModel saveLoadModel = new SaveLoadModel();
		private Control main, entryList, focusedEntryOptions, saveBttn;
		private Label label;
		private PopupController popup;
		private LoadEntryController focusedEntry, popupEntry;

		public override void _Ready()
		{
			main = GetChild<Control>(0);
			label = main.GetChild<Label>(0);
			entryList = main.GetNode<Control>("scrollContainer/entries");
			saveBttn = main.GetChild<Control>(2);

			focusedEntryOptions = GetChild<Control>(1);
			popupEntry = focusedEntryOptions.GetChild<LoadEntryController>(0);

			popup = GetChild<PopupController>(2);
			popup.Connect("hide", this, nameof(OnPopupHide));

			AddChild(saveLoadModel);
			foreach (int i in SaveLoadModel.saveFileNames.Keys)
			{
				AddEntry(i);
			}
			SetDisplay();
		}
		private void SaveGame()
		{
			int i;
			if (focusedEntry != null)
			{
				i = focusedEntry.saveIndex;
				saveLoadModel.SaveGame(i);
				focusedEntry.Display(SaveLoadModel.saveFileIcons[i], SaveLoadModel.saveFileNames[i]);
			}
			else
			{
				i = saveLoadModel.SaveGame();
				if (i != -1)
				{
					AddEntry(i);
				}
			}
		}
		private void AddEntry(int i)
		{
			LoadEntryController loadEntry = SceneDB.loadEntryScene.Instance<LoadEntryController>();
			loadEntry.saveIndex = i;

			entryList.AddChild(loadEntry);
			entryList.MoveChild(loadEntry, 0);

			loadEntry.Display(SaveLoadModel.saveFileIcons[i], SaveLoadModel.saveFileNames[i]);

			foreach (BaseButton button in loadEntry.buttons)
			{
				button.Connect("pressed", this, nameof(OnSaveSlotPressed),
					new Godot.Collections.Array() { loadEntry });
			}

			SetDisplay();
		}
		private void SetDisplay()
		{
			int currentSaveCount = SaveLoadModel.saveFileNames.Count;
			label.Text = string.Format("{0}/{1} Slots Used", currentSaveCount, SaveLoadModel.MAX_SAVES);
			saveBttn.Visible = currentSaveCount < SaveLoadModel.MAX_SAVES;
		}
		private void OnHide()
		{
			focusedEntryOptions.Visible = popup.Visible = false;
			main.Show();
			focusedEntry = null;
		}
		private void OnFocusedEntryBack()
		{
			PlaySound(NameDB.UI.CLICK3);
			OnHide();
		}
		private void OnPopupHide() { focusedEntryOptions.Show(); }
		private void OnSaveSlotPressed(LoadEntryController focusedEntry)
		{
			this.focusedEntry = focusedEntry;

			int i = focusedEntry.saveIndex;
			popupEntry.Display(SaveLoadModel.saveFileIcons[i], SaveLoadModel.saveFileNames[i]);

			PlaySound(NameDB.UI.CLICK2);
			main.Hide();
			focusedEntryOptions.Show();
		}
		private void OnDeletePressed()
		{
			PlaySound(NameDB.UI.CLICK1);
			focusedEntryOptions.Hide();
			popup.RouteConnection(nameof(OnDeleteConfirm), this);
			popup.ShowConfirm("Delete?");
		}
		private void OnDeleteConfirm()
		{
			saveLoadModel.DeleteSaveFile(focusedEntry.saveIndex);
			focusedEntry.QueueFree();
			OnHide();
			SetDisplay();
		}
		private void OnSavePressed()
		{
			PlaySound(NameDB.UI.CLICK1);
			if (focusedEntry == null)
			{
				OnSaveConfirm();
			}
			else
			{
				focusedEntryOptions.Hide();
				popup.RouteConnection(nameof(OnSaveConfirm), this);
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
			if (SaveLoadModel.dirty)
			{
				PlaySound(NameDB.UI.CLICK1);
				focusedEntryOptions.Hide();
				popup.RouteConnection(nameof(OnLoadConfirm), this);
				popup.ShowConfirm("Lose Save\nData?");
			}
			else
			{
				OnLoadConfirm();
			}
		}
		private void OnLoadConfirm()
		{
			PlaySound(NameDB.UI.CLICK0);
			saveLoadModel.LoadGame(focusedEntry.saveIndex);
		}
	}
}