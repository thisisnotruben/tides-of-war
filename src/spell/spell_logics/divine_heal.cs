using System;
using Godot;
namespace Game.Spell
{
    public class DivineHeal : Spell
    {
        public override float Cast()
        {
            GD.Randomize();
            caster.SetHp((short)Math.Round((float)caster.hpMax * GD.RandRange(0.1f, 0.3f)));
            return base.Cast();
        }
    }
}