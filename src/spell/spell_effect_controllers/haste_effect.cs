namespace Game.Ability
{
	public class haste_effect : cleave_effect
	{
		public override void Init(Actor.Character character)
		{
			base.Init(character);
			lightFadeDelay = 0.8f;
		}
	}
}