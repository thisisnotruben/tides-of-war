using System;
using System.Collections.Generic;
using Game.Actor;
using Godot;
namespace Game.Ability
{
    public class intimidating_shout : Spell
    {
        private Dictionary<Character, Tuple<short, short>> targetList = new Dictionary<Character, Tuple<short, short>>();
        public override float Cast()
        {
            if (loaded)
            {
                foreach (Character character in targetList.Keys)
                {
                    character.minDamage -= targetList[character].Item1;
                    character.maxDamage -= targetList[character].Item2;
                }
            }
            else
            {
                foreach (Area2D characterArea2D in GetNode<Area2D>("sight").GetOverlappingAreas())
                {
                    Character character = characterArea2D.Owner as Character;
                    if (character != null && character != caster)
                    {
                        short amount1 = (short)Math.Round((float)character.minDamage * 0.20f);
                        short amount2 = (short)Math.Round((float)character.maxDamage * 0.20f);
                        targetList.Add(character, new Tuple<short, short>(amount1, amount2));
                    }
                }
            }
            SetTime(3.0f);
            return base.Cast();
        }
        public override void _OnTimerTimeout()
        {
            foreach (Character character in targetList.Keys)
            {
                character.minDamage += targetList[character].Item1;
                character.maxDamage += targetList[character].Item2;
            }
            UnMake();
        }
    }
}