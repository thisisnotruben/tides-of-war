using System;
using Godot;
using Game.Database;
namespace Game.Ui
{
	public class SlotController : Control
	{
		private Tween tween;
		private ColorRect cooldownOverlay;
		private Label coolDownText, stackCount;
		private TextureRect icon;
		private BaseButton _button;
		public BaseButton button { get { return _button; } }

		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			cooldownOverlay = GetNode<ColorRect>("margin/cooldownOverlay");
			coolDownText = GetNode<Label>("margin/cooldownText");
			stackCount = GetNode<Label>("stackCount");
			icon = GetNode<TextureRect>("margin/icon");
			_button = GetNode<BaseButton>("button");

			// default display
			ClearDisplay();
		}
		public void Display(string worldName, int currentStack)
		{
			// display stack count and icon
			icon.Texture = PickableDB.GetIcon(worldName);
			stackCount.Text = currentStack.ToString();
			stackCount.Visible = currentStack > 1;
		}
		public void ClearDisplay()
		{
			icon.Texture = null;
			stackCount.Text = "0";
			stackCount.Hide();
			coolDownText.Hide();
			cooldownOverlay.Hide();
			tween.RemoveAll();
		}
		public void SetCooldown(float time)
		{
			if (time == 0.0f)
			{
				return;
			}

			coolDownText.Show();
			cooldownOverlay.Show();

			tween.InterpolateMethod(this, nameof(SetCooldownText),
				time, 0.0f, time,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		public void OnTweenCompleted(Godot.Object gObject, NodePath key)
		{
			coolDownText.Hide();
			cooldownOverlay.Hide();
		}
		public void OnButtonChanged(bool down) { icon.RectScale = (down) ? new Vector2(0.8f, 0.8f) : Vector2.One; }
		private void SetCooldownText(float time) { coolDownText.Text = Math.Round(time, 0).ToString(); }
	}
}