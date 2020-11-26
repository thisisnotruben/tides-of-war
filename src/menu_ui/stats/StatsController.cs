using Godot;
using Game.Database;
namespace Game.Ui
{
	public class StatsController : GameMenu
	{
		protected Control mainContent;
		public ItemInfoController itemInfoController;
		protected RichTextLabel textLabel;
		protected TextureRect weaponIcon, armorIcon;

		public override void _Ready()
		{
			mainContent = GetNode<Control>("s");

			itemInfoController = GetNode<ItemInfoController>("item_info");
			itemInfoController.itemList = null;
			itemInfoController.Connect("hide", this, nameof(_OnStatsNodeDraw));

			textLabel = GetNode<RichTextLabel>("s/v/c/label");
			weaponIcon = GetNode<TextureRect>("s/v/slots/weapon/m/icon");
			armorIcon = GetNode<TextureRect>("s/v/slots/armor/m/icon");

			TextureButton weaponSlot = GetNode<TextureButton>("s/v/slots/weapon"),
				armorSlot = GetNode<TextureButton>("s/v/slots/armor");

			weaponSlot.Connect("pressed", this, nameof(_OnEquippedSlotPressed),
				new Godot.Collections.Array() { true });
			armorSlot.Connect("pressed", this, nameof(_OnEquippedSlotPressed),
				new Godot.Collections.Array() { false });

			// connect button effects
			weaponSlot.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { weaponSlot, true });
			armorSlot.Connect("button_down", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { armorSlot, true });

			weaponSlot.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { weaponSlot, false });
			armorSlot.Connect("button_up", this, nameof(OnSlotMoved),
				new Godot.Collections.Array() { armorSlot, false });
		}
		public void _OnStatsNodeDraw()
		{
			weaponIcon.Texture = player.weapon == null ? null : PickableDB.GetIcon(player.weapon.worldName);
			armorIcon.Texture = player.vest == null ? null : PickableDB.GetIcon(player.vest.worldName);

			mainContent.Show();
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
		public void _OnEquippedSlotPressed(bool weapon)
		{
			if ((weapon && player.weapon != null) || (!weapon && player.vest != null))
			{
				mainContent.Hide();
				itemInfoController.Display(weapon ? player.weapon.worldName : player.vest.worldName, false);
			}
		}
	}
}