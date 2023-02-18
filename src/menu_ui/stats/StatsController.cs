using Godot;
namespace Game.Ui
{
	public class StatsController : GameMenu
	{
		private RichTextLabel richTextLabel;
		private TextureRect portrait;
		private Label characterName;

		public override void _Ready()
		{
			richTextLabel = GetNode<RichTextLabel>("richTextLabel");
			portrait = GetNode<TextureRect>("hBoxContainer/portrait");
			characterName = GetNode<Label>("hBoxContainer/name");
		}
		public void OnDraw()
		{
			Texture texture = player.img.Texture, portraitTex = null;
			string imgName = texture.ResourcePath.GetFile().BaseName();

			if (Globals.imageDB.HasData(imgName))
			{
				AtlasTexture atlasTexture = new AtlasTexture();
				atlasTexture.Atlas = texture;
				atlasTexture.Region = new Rect2(Vector2.Zero,
					new Vector2(texture.GetWidth() / Globals.imageDB.GetData(imgName).total,
						texture.GetHeight()));

				portraitTex = atlasTexture;
			}

			portrait.Texture = portraitTex;
			characterName.Text = player.Name;

			richTextLabel.BbcodeText =
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