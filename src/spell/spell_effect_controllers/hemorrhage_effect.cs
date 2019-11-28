namespace Game.Ability
{
    public class hemorrhage_effect : cleave_effect
    {
        public override void Init(Actor.Character character)
        {
            base.Init(character);
            playSound = true;
        }
    }
}