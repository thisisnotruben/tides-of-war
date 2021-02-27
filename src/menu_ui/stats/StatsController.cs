using Godot;
namespace Game.Ui
{
	public class StatsController : GameMenu
	{
		public override void _Ready()
		{
			base._Ready();
			OnDraw();
		}
		private void OnDraw()
		{
			if (player == null)
			{
				return;
			}

			GetChild<RichTextLabel>(0).BbcodeText =
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
	}
}