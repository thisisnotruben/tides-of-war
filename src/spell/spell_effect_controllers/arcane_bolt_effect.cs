namespace Game.Ability
{
	public class arcane_bolt_effect : fireball_effect
	{
		public override void Init(Actor.Character character)
		{
			base.Init(character);
			lightFadeDelay = 2.0f;
		}
	}
}