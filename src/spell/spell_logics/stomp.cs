using System.Collections.Generic;
using Game.Actor;
using Godot;
namespace Game.Ability
{
    public class stomp : Spell
    {
        private List<Character> targetList = new List<Character>();
        public override float Cast()
        {
            if (loaded)
            {
                foreach (Character character in targetList)
                {
                    StunUnit(character, true);
                }
            }
            else
            {
                foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
                {
                    Character character = characterArea2D.Owner as Character;
                    if (character != null && character != caster)
                    {
                        character.TakeDamage(10, false, caster, Game.Actor.Doodads.CombatText.TextType.HIT);
                        GD.Randomize();
                        if (GD.Randi() % 100 + 1 > 20)
                        {
                            StunUnit(character, true);
                            targetList.Add(character);
                        }
                    }
                }
            }
            SetTime(3.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            foreach (Character character in targetList)
            {
                StunUnit(character, false);
            }
            UnMake();
        }
    }
}