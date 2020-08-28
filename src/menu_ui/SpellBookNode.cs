using Godot;
namespace Game.Ui
{
	public class SpellBookNode : GameMenu
	{
		public ItemList itemList;
		private ItemInfoNodeSpell itemInfoNodeSpell;
		private Popup popup;

		public override void _Ready()
		{
			itemList = GetNode<ItemList>("s/v/c/spell_list");
			itemInfoNodeSpell = GetNode<ItemInfoNodeSpell>("item_info");
			itemInfoNodeSpell.itemList = itemList;
			itemInfoNodeSpell.Connect("hide", this, nameof(_OnSpellBookNodeHide));
			popup = GetNode<Popup>("popup");
			popup.Connect("hide", this, nameof(_OnSpellBookNodeHide));
		}
		public void _OnSpellBookNodeDraw()
		{
			GetNode<Label>("s/v/m/v/player_hp_header").Text = $"Health: {player.hp} / {player.stats.hpMax.valueI}";
			GetNode<Label>("s/v/m/v/player_mana_header").Text = $"Mana: {player.mana} / {player.stats.manaMax.valueI}";
		}
		public void _OnSpellBookNodeHide()
		{
			popup.Hide();
			GetNode<Control>("s").Show();
		}
		public void _OnSpellBookIndexSelected(int index)
		{
			Globals.PlaySound("spell_select", this, speaker);
			GetNode<Control>("s").Hide();
			itemInfoNodeSpell.Display(itemList.GetItemMetaData(index), true);
		}
	}
}
