namespace Game.Spell
{
    public class MeteorEffect : FireballEffect
    {
        public override void _Ready()
        {
            base._Ready();
            SetProcess(true);
        }
    }
}