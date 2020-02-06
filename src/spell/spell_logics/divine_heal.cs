using System;
using Godot;
namespace Game.Ability
{
    public class divine_heal : Spell
    {
        public override float Cast()
        {
            GD.Randomize();
            caster.hp = (short)Math.Round((float)caster.hpMax * GD.RandRange(0.1f, 0.3f));
            return base.Cast();
        }
    }
}