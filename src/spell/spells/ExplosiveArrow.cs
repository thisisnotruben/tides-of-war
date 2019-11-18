using Godot;
using Game.Actor;

namespace Game.Spell
{
    public class ExplosiveArrow : Spell
    {
        public override float Cast()
        {
            foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
            {
                Character character = characterArea2D.GetOwner() as Character;
                if (character != null && character != caster)
                {
                    character.TakeDamage(10, false, caster, Game.Misc.Other.CombatText.TextType.HIT);
                }
            }
            return base.Cast();
        }
        public override void ConfigureSpell()
        {
            PrepSight();
        }
    }
}