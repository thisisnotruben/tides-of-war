
namespace Game.Spell
{
    public class Devastate : Spell
    {
        
        public override float Cast()
        {
            casted = true;
            return 1.1f;
        }
        public override bool Casted()
        {
            return casted;
        }
        public override void Make()
        {
            base.Make();
            SetWorldType(WorldTypes.DEVASTATE);
            effectOnTarget = true;
        }
    }
}