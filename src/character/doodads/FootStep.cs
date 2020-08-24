using Godot;
namespace Game.Actor.Doodads
{
	public class FootStep : Node2D
	{
		private Vector2 StepPos = new Vector2();
		public override void _Draw()
		{
			DrawRect(new Rect2(StepPos, new Vector2(3.0f, 3.0f)), new Color(0.10f, 0.10f, 0.10f, 0.175f));
		}
		public void _OnTimerTimeout()
		{
			Tween tween = GetNode<Tween>("tween");
			tween.InterpolateProperty(this, ":modulate", Modulate,
				new Color(0.15f, 0.15f, 0.15f, 0.0f), 2.0f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
			tween.Start();
		}
		public void SetStep(Vector2 StepPos)
		{
			this.StepPos = StepPos;
		}
		public void _OnTweenCompleted(Godot.Object obj, NodePath nodePath)
		{
			QueueFree();
		}
	}
}