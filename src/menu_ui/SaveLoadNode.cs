using Godot;
using Game.Utils;
namespace Game.Ui
{
    public class SaveLoadNode : Control
    {
        private Speaker _speaker;
        public Speaker speaker
        {
            set
            {
                _speaker = value;
                popup.speaker = value;
            }
            get
            {
                return _speaker;
            }
        }
        private Popup popup;
        private int index;

        public override void _Ready()
        {
            popup = GetNode<Popup>("popup");
            popup.Connect("hide", this, nameof(_OnSaveLoadNodeHide));
            SetLabels();
        }
        private void SaveGame()
        {
            Godot.Collections.Dictionary date = OS.GetDatetime();
            string time = $"{date["month"]}-{date["day"]} {date["hour"]}:{date["minute"]}";
            Globals.SaveGameData(time, index);
            Globals.SaveGame(Globals.SAVE_PATH[$"SAVE_SLOT_{index}"]);
            GetLabel(index).Text = time;
        }
        private Label GetLabel(int index)
        {
            return GetNode<Label>($"v/s/c/g/slot_label_{index}");
        }
        private void SetLabels()
        {
            int labelIdx = 0;
            foreach (Node node in GetNode<Node>("v/s/c/g").GetChildren())
            {
                Label label = node as Label;
                if (label != null)
                {
                    string slotText = $"slot_{labelIdx}";
                    if (Globals.saveData.ContainsKey(slotText) && !Globals.saveData[slotText].Empty())
                    {
                        GetLabel(labelIdx++).Text = Globals.saveData[slotText];
                    }
                }
            }
        }
        public void RouteConnections(string toMethod)
        {
            // TODO: need to clear pervious signal list before connecting
            popup.GetNode("m/yes_no/yes").Connect("pressed", this, toMethod);
        }
        public void _OnSaveLoadNodeHide()
        {
            popup.Hide();
            GetNode<Control>("v").Show();
        }
        public void _OnPopupHide()
        {
            popup.GetNode<Control>("m/save_load/delete").Hide();
            popup.GetNode<Control>("m/save_load/save").Hide();
            popup.GetNode<Control>("m/save_load/load").Hide();
        }
        public void _OnSlotPressed(int index)
        {
            this.index = index;
            Globals.PlaySound("click2", this, speaker);
            GetNode<Control>("v").Hide();
            popup.GetNode<Control>("m/save_load/save").Show();
            if (!GetLabel(index).Text.Equals($"Slot {index + 1}"))
            {
                popup.GetNode<Control>("m/save_load/load").Show();
                popup.GetNode<Control>("m/save_load/delete").Show();
            }
            popup.GetNode<Control>("m/save_load").Show();
            popup.Show();
        }
        public void _OnDeletePressed()
        {
            Globals.PlaySound("click1", this, speaker);
            RouteConnections(nameof(_OnDeleteConfirm));
            popup.GetNode<Label>("m/yes_no/label").Text = "Delete?";
            popup.GetNode<Control>("m/yes_no").Show();
            popup.Show();
        }
        public void _OnDeleteConfirm()
        {
            Globals.SaveGameData("", index);
            GetLabel(index).Text = $"Slot {index + 1}";
            new Directory().Remove(Globals.SAVE_PATH[$"SAVE_SLOT_{index}"]);
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
                popup.GetNode<Label>("m/yes_no/label").Text = "Overwrite?";
                popup.GetNode<Control>("m/yes_no").Show();
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
            Globals.LoadGame(Globals.SAVE_PATH[$"SAVE_SLOT_{index}"]);
            Hide();
        }
        public void _OnBackPressed()
        {
            Globals.PlaySound("click3", this, speaker);
            Hide();
        }
    }
}
