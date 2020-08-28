namespace Game.Ability
{
	public class siphon_mana_effect : fireball_effect
	{
		public override void Init(Actor.Character character)
		{
			base.Init(character);
			lightFadeDelay = 1.0f;
		}
		public override void OnHit(Spell spell = null)
		{
			base.OnHit(spell);
			GlobalPosition = character.target.pos;
		}
	}
}