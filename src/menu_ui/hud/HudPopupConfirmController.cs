using Godot;
namespace Game.Ui
{
	public class HudPopupConfirmController : CenterContainer
	{
		private Tween tween;
		private Control container;
		public Button button;

		public override void _Ready()
		{
			tween = GetChild<Tween>(0);
			container = GetChild<Control>(1);
			button = container.GetNode<Button>("marginContainer/button");
		}
		public void ShowConfirm(string confirmText)
		{
			button.Text = confirmText;
			RectMinSize = container.RectSize;

			Show();
			tween.InterpolateProperty(this, "modulate",
					Color.ColorN("white", 0.0f), Color.ColorN("white"),
					0.3f, Tween.TransitionType.Quint, Tween.EaseType.Out);
			tween.Start();
		}
		public void HideConfirm()
		{
			tween.InterpolateProperty(this, "modulate",
					Modulate, Color.ColorN("white", 0.0f),
					0.3f, Tween.TransitionType.Quint, Tween.EaseType.Out);
			tween.Start();
		}
		private void OnConfirm() { HideConfirm(); }
		private void OnTweenAllCompleted() { Visible = !Modulate.Equals(Color.ColorN("white", 0.0f)); }
	}
}