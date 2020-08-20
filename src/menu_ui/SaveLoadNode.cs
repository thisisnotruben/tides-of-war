using Godot;
namespace Game.Ui
{
    public class SaveLoadNode : GameMenu
    {
        private readonly Archiver archiver = new Archiver();
        private Popup popup;
        private int index;

        public override void _Ready()
        {
            AddChild(archiver);
            popup = GetNode<Popup>("popup");
            popup.Connect("hide", this, nameof(_OnSaveLoadNodeHide));
            popup.GetNode<BaseButton>("m/save_load/save").Connect("pressed", this, nameof(_OnSavePressed));
            SetLabels();
        }
        private void SaveGame()
        {
            archiver.SaveGame(index);
            GetLabel(index - 1).Text = Archiver.GetSaveFileName(index);
        }
        private Label GetLabel(int index)
        {
            return GetNode<Label>($"v/s/c/g/slot_label_{index}");
        }
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
            this.index = index + 1;
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
            archiver.LoadGame(index);
            Hide();
        }
    }
}
