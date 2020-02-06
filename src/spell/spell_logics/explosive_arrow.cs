using Game.Actor;
using Godot;
namespace Game.Ability
{
    public class explosive_arrow : Spell
    {
        public override float Cast()
        {
            foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
            {
                Character character = characterArea2D.Owner as Character;
                if (character != null && character != caster)
                {
                    character.TakeDamage(10, false, caster, Game.Actor.Doodads.CombatText.TextType.HIT);
                }
            }
            return base.Cast();
        }
        public override void ConfigureSpell()
        {
            caster.SetCurrentSpell(this);
            caster.SetState(Character.States.IDLE);
            PrepSight();
        }
    }
}