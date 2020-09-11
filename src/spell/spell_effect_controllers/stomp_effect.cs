namespace Game.Ability
{
	public class stomp_effect : SpellEffect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			tween.Start();
			timer.Start();
		}
	}
}