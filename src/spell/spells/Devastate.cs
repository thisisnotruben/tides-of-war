
namespace Game.Spell
{
    public class Devastate : Spell
    {
        public Devastate(WorldTypes worldType) : base(worldType) { }

        public override float Cast()
        {
            return 1.1f;
        }
        public override bool Casted()
        {
            return casted;
        }
    }
}