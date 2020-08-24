namespace Game.Ability
{
	public class sniper_shot_effect : explosive_arrow_effect
	{
		public override void Init(Actor.Character character)
		{
			base.Init(character);
			playSound = true;
		}
	}
}