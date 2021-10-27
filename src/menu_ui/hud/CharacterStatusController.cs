using Game.Actor;
using Godot;
namespace Game.Ui
{
	public class CharacterStatusController : Control
	{
		private static float NAME_DELAY_TIME = 1.3f,
			NAME_UPDATE_TIME = 0.37f,
			NAME_REFRESH_TIME = 7.5f;

		private Label nameHeader, healthBarHeader, manaBarHeader;
		private Range healthBar, manaBar;
		private Timer timer;

		private Character characterFocused;
		public bool showPercent = false;
		private string nameText = string.Empty;
		private int i = 0;

		public override void _Ready()
		{
			nameHeader = GetNode<Label>("nameContainer/margin/unitName");
			healthBarHeader = GetNode<Label>("healthHeader");
			manaBarHeader = GetNode<Label>("manaHeader");
			healthBar = healthBarHeader.GetNode<Range>("healthBar");
			manaBar = manaBarHeader.GetNode<Range>("manaBar");
			timer = GetNode<Timer>("timer");
			timer.Connect("timeout", this, nameof(OnTimerTimeout));
		}
		public void ConnectCharacterStatusAndUpdate(Character character)
		{
			Clear();

			if (character != null)
			{
				character.Connect(nameof(Character.UpdateHudHealthStatus), this, nameof(UpdateHealth));
				character.Connect(nameof(Character.UpdateHudManaStatus), this, nameof(UpdateMana));

				UpdateHealth(character.hp, character.stats.hpMax.valueI);
				UpdateMana(character.mana, character.stats.manaMax.valueI);

				nameHeader.Text = nameText = character.worldName;
				if (nameHeader.GetFont("font").GetStringSize(nameText).x > nameHeader.RectSize.x)
				{
					nameText += "  ";
					nameHeader.Align = Label.AlignEnum.Left;
					timer.Start(NAME_DELAY_TIME);
				}

				characterFocused = character;
				Show();
			}
		}
		public void Clear(bool show = true)
		{
			if (characterFocused != null)
			{
				characterFocused.Disconnect(nameof(Character.UpdateHudHealthStatus), this, nameof(UpdateHealth));
				characterFocused.Disconnect(nameof(Character.UpdateHudManaStatus), this, nameof(UpdateMana));
			}

			timer.Stop();
			nameHeader.Align = Label.AlignEnum.Center;
			i = 0;

			characterFocused = null;
			Visible = show;
		}
		private void UpdateStatus(Range progress, Label label, int current, int max)
		{
			double value = (double)current / (double)max;

			progress.Value = value;
			label.Text = showPercent ? $"{(value * 100.0).ToString("0.")}%" : $"{current}/{max}";
		}
		public void UpdateHealth(int current, int max) { UpdateStatus(healthBar, healthBarHeader, current, max); }
		public void UpdateMana(int current, int max) { UpdateStatus(manaBar, manaBarHeader, current, max); }
		public bool IsCharacterConnected(Character character) { return character == characterFocused; }
		private void OnTimerTimeout()
		{
			i = (i + 1) % nameText.Length;
			nameHeader.Text = nameText.Substring(i, nameText.Length - i) + nameText;

			if (i == 0)
			{
				timer.Start(NAME_REFRESH_TIME);
			}
			else if (timer.WaitTime != NAME_UPDATE_TIME)
			{
				timer.Start(NAME_UPDATE_TIME);
			}
		}
	}
}