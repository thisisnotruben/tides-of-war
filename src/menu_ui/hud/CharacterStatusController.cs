using Game.Actor;
using Godot;
public class CharacterStatusController : Control
{
	private Label nameHeader, healthBarHeader, manaBarHeader;
	private Range healthBar, manaBar;

	private Character characterFocused;
	public bool showPercent = false;

	public override void _Ready()
	{
		nameHeader = GetNode<Label>("nameContainer/margin/unitName");
		healthBarHeader = GetNode<Label>("healthHeader");
		manaBarHeader = GetNode<Label>("manaHeader");
		healthBar = healthBarHeader.GetNode<Range>("healthBar");
		manaBar = manaBarHeader.GetNode<Range>("manaBar");
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
			nameHeader.Text = character.worldName;

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
}