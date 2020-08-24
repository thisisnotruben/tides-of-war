using Godot;
namespace Game.Ability
{
	public class bash_effect : SpellEffect
	{
		public override void Init(Actor.Character character)
		{
			base.Init(character);
			fadeLight = false;
		}
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			Position = character.GetNode<Node2D>("head").Position;
			tween.Start();
			timer.Start();
		}
		public override void _OnTimerTimeout()
		{
			base._OnTimerTimeout();
			FadeLight(true);
			tween.InterpolateProperty(this, ":modulate", Modulate,
				new Color(1.0f, 1.0f, 1.0f, 0.0f), lightFadeDelay,
				Tween.TransitionType.Linear, Tween.EaseType.InOut);
			tween.Start();
			foreach (Particles2D particles2D in GetNode("idle").GetChildren())
			{
				particles2D.Emitting = false;
			}
			timer.Start();
		}
	}
}