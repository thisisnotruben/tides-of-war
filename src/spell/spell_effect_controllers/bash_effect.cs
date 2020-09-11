using Godot;
namespace Game.Ability
{
	public class bash_effect : stomp_effect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			Position = character.head.Position;
		}
		public override void _OnTimerTimeout()
		{
			base._OnTimerTimeout();
			FadeLight(true);
			tween.InterpolateProperty(this, ":modulate", Modulate,
				new Color("00ffffff"), lightFadeDelay,
				Tween.TransitionType.Linear, Tween.EaseType.InOut);
			tween.Start();
			foreach (Particles2D particles2D in idleParticles.GetChildren())
			{
				particles2D.Emitting = false;
			}
			timer.Start();
		}
	}
}