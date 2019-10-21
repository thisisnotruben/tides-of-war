using Godot;

namespace Game.Ui
{
    public class SaveLoad : Control
    {
        public override void _Ready()
        {
            SetLabels();
            if (GetOwner() is StartMenu)
            {
                GetNode<Label>("v/label").SetText("Load Game");
            }
        }
        public void _OnSlotPressed(int index)
        {
            ((Menu)GetOwner()).popup.SaveLoadGo(index);
        }
        public void SetLabels()
        {
            for (int i = 0; i < GetNode<Node>("v/s/c/g").GetChildCount() / 2; i++)
            {
                string slotText = string.Format("slot_{0}", i);
                if (Globals.gameMeta.ContainsKey(slotText) && !((string)Globals.gameMeta[slotText]).Empty())
                {
                    GetNode<Label>(string.Format("v/s/c/g/slot_label_{0}", i)).SetText((string)Globals.gameMeta[slotText]);
                }
            }
        }
    }
}