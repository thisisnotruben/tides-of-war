using Godot;
namespace Game.Ui
{
	public class StatsNode : GameMenu
	{
		private ItemInfoNode itemInfoNode;

		public override void _Ready()
		{
			itemInfoNode = GetNode<ItemInfoNode>("item_info");
			itemInfoNode.itemList = null;
			itemInfoNode.Connect("hide", this, nameof(Show));
			BaseButton addToHudBttn = itemInfoNode.GetNode<BaseButton>("s/v/c/v/add_to_hud");
			addToHudBttn.Disconnect("button_up", itemInfoNode, nameof(ItemInfoNode._OnSlotMoved));
			addToHudBttn.Disconnect("button_down", itemInfoNode, nameof(ItemInfoNode._OnSlotMoved));
			addToHudBttn.Disconnect("pressed", itemInfoNode, nameof(ItemInfoNode._OnAddToHudPressed));
		}
		public void _OnStatsNodeDraw()
		{
			GetNode<RichTextLabel>("s/v/c/label").BbcodeText =
				$"Name: {player.worldName}\n" +
				$"Health: {player.hp} / {player.hpMax}\n" +
				$"Mana: {player.mana} / {player.manaMax}\n" +
				$"XP: {player.xp}\n" +
				$"Level: {player.level}\n" +
				$"Gold: {player.gold}\n" +
				$"Stamina: {player.stamina}\n" +
				$"Intellect: {player.intellect}\n" +
				$"Agility: {player.agility}\n" +
				$"Armor: {player.armor}\n" +
				$"Damage: {player.minDamage} - {player.maxDamage}\n" +
				$"Attack Speed: {player.weaponSpeed.ToString("0.00")}\n" +
				$"Attack Range: {player.weaponRange}";
		}
		public void _OnEquippedSlotMoved(string nodePath, bool down)
		{
			float scale = (down) ? 0.8f : 1.0f;
			GetNode<Control>(nodePath).RectScale = new Vector2(scale, scale);
		}
		public void _OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player.weapon != null) || (!weapon && player.vest != null))
			{
				itemInfoNode.Display((weapon) ? player.weapon.worldName : player.vest.worldName, false);
			}
		}
	}
}