using Godot;
namespace Game.Ability
{
	public class frenzy_effect : SpellEffect
	{
		public override void OnHit(Spell spell)
		{
			base.OnHit(spell);
			Position = character.head.Position;
			character.GetParent().MoveChild(this, 0);
			tween.Start();
			timer.Start();
		}
		public override void _OnTimerTimeout()
		{
			base._OnTimerTimeout();
			FadeLight(true);
			foreach (Particles2D particles2D in idleParticles.GetChildren())
			{
				particles2D.Emitting = false;
			}
			timer.Start();
		}
	}
}