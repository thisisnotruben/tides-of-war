using Godot;
namespace Game.Ability
{
	public class cleave_effect : SpellEffect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			Position = character.GetNode<Node2D>("img").Position;
			tween.Start();
			timer.Start();
		}
		public override void _OnTimerTimeout()
		{
			base._OnTimerTimeout();
			QueueFree();
		}
	}
}