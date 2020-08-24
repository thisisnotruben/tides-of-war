namespace Game.Ability
{
	public class searing_arrow_effect : stomp_effect
	{
		public override void Init(Actor.Character character)
		{
			base.Init(character);
			playSound = true;
		}
	}
}