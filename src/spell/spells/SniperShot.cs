using System;

namespace Game.Spell
{
    public class SniperShot : Spell
    {
        ushort amount;

        public override float Cast()
        {
            caster.weaponRange -= amount;
            return base.Cast();
        }
        public override void ConfigureSpell()
        {
            amount = (ushort)Math.Round(caster.weaponRange * 0.25f);
            caster.weaponRange += amount;
        }
    }
}