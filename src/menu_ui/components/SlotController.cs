using System;
using Godot;
using Game.Database;
namespace Game.Ui
{
	public class SlotController : Control
	{
		protected Tween tween;
		protected ColorRect cooldownOverlay;
		protected Label coolDownText, stackCount;
		protected TextureRect icon;
		protected BaseButton _button;
		public BaseButton button { get { return _button; } }
		protected bool coolDownActive;
		protected string commodityWorldName;

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
			commodityWorldName = worldName;
			icon.Texture = PickableDB.GetIcon(worldName);
			stackCount.Text = currentStack.ToString();
			stackCount.Visible = currentStack > 1;
		}
		public void ClearDisplay()
		{
			icon.Texture = null;
			stackCount.Text = commodityWorldName = string.Empty;
			cooldownOverlay.Visible = coolDownText.Visible = stackCount.Visible = coolDownActive = false;
			tween.RemoveAll();
		}
		public void SetCooldown(float time)
		{
			if ((int)time <= 0)
			{
				return;
			}

			tween.RemoveAll();
			cooldownOverlay.Visible = coolDownText.Visible = coolDownActive = true;

			tween.InterpolateMethod(this, nameof(SetCooldownText),
				time, 0.0f, time,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
		}
		public bool HasCommodity(string worldName) { return !commodityWorldName.Empty() && worldName.Equals(commodityWorldName); }
		public bool IsAvailable() { return icon.Texture == null; }
		public void OnTweenCompleted(Godot.Object gObject, NodePath key) { cooldownOverlay.Visible = coolDownText.Visible = false; }
		public void OnButtonChanged(bool down) { icon.RectScale = down ? new Vector2(0.8f, 0.8f) : Vector2.One; }
		protected void SetCooldownText(float time) { coolDownText.Text = Math.Round(time, 0).ToString(); }
	}
}