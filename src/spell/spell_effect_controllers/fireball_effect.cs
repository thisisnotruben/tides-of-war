using Godot;
namespace Game.Ability
{
	public class fireball_effect : SpellEffect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			Node2D bolt = idleParticles.GetNode<Node2D>("bolt");
			tween.InterpolateProperty(bolt, ":modulate", bolt.Modulate,
				new Color("00ffffff"), lightFadeDelay,
				Tween.TransitionType.Linear, Tween.EaseType.In);
			tween.Start();
			timer.Start();
		}
	}
}