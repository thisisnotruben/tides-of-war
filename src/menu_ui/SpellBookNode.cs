namespace Game.Ui
{
    public class SpellBookNode : GameMenu
    {
        private ItemList itemList;
        private ItemInfoNodeSpell itemInfoNodeSpell;

        public override void _Ready()
        {
            itemList = GetNode<ItemList>("s/v/c/spell_list");
            itemInfoNodeSpell = GetNode<ItemInfoNodeSpell>("item_info");
            itemInfoNodeSpell.itemList = itemList;
            itemInfoNodeSpell.Connect("hide", this, nameof(_OnItemInfoHide));
        }
        public void _OnItemInfoHide()
        {
            Show();
        }
        public void _OnBagIndexSelected(int index)
        {
            Globals.PlaySound("spell_select", this, speaker);
            string pickableWorldName = itemList.GetItemMetaData(index);
            Hide();
            itemInfoNodeSpell.Display(pickableWorldName, true);
        }
    }
}
