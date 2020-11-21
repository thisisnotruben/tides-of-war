using Godot;
namespace Game.Ui
{
	public class StatsController : GameMenu
	{
		protected ItemInfoController itemInfoController;
		protected RichTextLabel textLabel;

		public override void _Ready()
		{
			itemInfoController = GetNode<ItemInfoController>("item_info");
			itemInfoController.itemList = null;
			itemInfoController.Connect("hide", this, nameof(Show));

			textLabel = GetNode<RichTextLabel>("s/v/c/label");

			BaseButton addToHudBttn = itemInfoController.GetNode<BaseButton>("s/v/c/v/add_to_hud");
			addToHudBttn.Disconnect("button_up", itemInfoController, nameof(ItemInfoController._OnSlotMoved));
			addToHudBttn.Disconnect("button_down", itemInfoController, nameof(ItemInfoController._OnSlotMoved));
			addToHudBttn.Disconnect("pressed", itemInfoController, nameof(ItemInfoController._OnAddToHudPressed));
		}
		public void _OnStatsNodeDraw()
		{
			textLabel.BbcodeText =
				$"Name: {player.worldName}\n" +
				$"Health: {player.hp} / {player.stats.hpMax.valueI}\n" +
				$"Mana: {player.mana} / {player.stats.manaMax.valueI}\n" +
				$"XP: {player.xp}\n" +
				$"Level: {player.level}\n" +
				$"Gold: {player.gold}\n" +
				$"Stamina: {player.stats.stamina.valueI}\n" +
				$"Intellect: {player.stats.intellect.valueI}\n" +
				$"Agility: {player.stats.agility.valueI}\n" +
				$"Armor: {player.stats.armor.valueI}\n" +
				$"Damage: {player.stats.minDamage.valueI} - {player.stats.maxDamage.valueI}\n" +
				$"Attack Speed: {player.stats.weaponSpeed.value.ToString("0.00")}\n" +
				$"Attack Range: {player.stats.weaponRange.value}";
		}
		public void _OnEquippedSlotMoved(string nodePath, bool down)
		{
			float scale = down ? 0.8f : 1.0f;
			GetNode<Control>(nodePath).RectScale = new Vector2(scale, scale);
		}
		public void _OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player.weapon != null) || (!weapon && player.vest != null))
			{
				itemInfoController.Display((weapon) ? player.weapon.worldName : player.vest.worldName, false);
			}
		}
	}
}