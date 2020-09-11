namespace Game.Ability
{
	public class siphon_mana_effect : fireball_effect
	{
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			GlobalPosition = character.target.pos;
		}
	}
}