using Godot;
namespace Game.Ui
{
	public class HudPopupErrorController : Control
	{
		private Label label;
		private Timer timer;
		private Tween tween;

		public override void _Ready()
		{
			label = GetChild<Label>(0);
			timer = GetChild<Timer>(1);
			tween = GetChild<Tween>(2);
		}
		public void ShowError(string errorText)
		{
			errorText = errorText.Replace("\n", " ");

			label.Text = errorText;
			// size + padding
			label.RectMinSize = label.GetFont("font").GetStringSize(errorText) + new Vector2(16.0f, 16.0f);

			Show();
			tween.InterpolateProperty(this, "modulate",
				Color.ColorN("white", 0.0f), Color.ColorN("white"),
				0.3f, Tween.TransitionType.Quint, Tween.EaseType.Out);
			tween.Start();
			timer.Start(1.0f);
		}
		private void OnTimerTimeout()
		{
			tween.InterpolateProperty(this, "modulate",
				Modulate, Color.ColorN("white", 0.0f),
				0.3f, Tween.TransitionType.Quint, Tween.EaseType.Out);
			tween.Start();
		}
		private void OnTweenAllCompleted() { Visible = !timer.IsStopped(); }
	}
}