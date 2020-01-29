using Godot;
namespace Game.Ui
{
    public class SaveLoad : Control
    {
        public override void _Ready()
        {
            SetLabels();
            if (Owner is StartMenu)
            {
                GetNode<Label>("v/label").Text = "Load Game";
            }
        }
        public void _OnSlotPressed(int index)
        {
            ((Menu)Owner).popup.SaveLoadGo(index);
        }
        public void SetLabels()
        {
            for (int i = 0; i < GetNode<Node>("v/s/c/g").GetChildCount() / 2; i++)
            {
                string slotText = $"slot_{i}";
                if (Globals.saveData.ContainsKey(slotText) && !Globals.saveData[slotText].Empty())
                {
                    GetNode<Label>($"v/s/c/g/slot_label_{i}").Text = Globals.saveData[slotText];
                }
            }
        }
    }
}