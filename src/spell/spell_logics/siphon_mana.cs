using System;
using Game.Actor.Doodads;
using Godot;
namespace Game.Ability
{
    public class siphon_mana : Spell
    {
        public override float Cast()
        {
            int mana = (int)Math.Round((float)caster.target.mana * 0.2f);
            caster.target.mana = -mana;
            caster.mana = mana;
            PackedScene CombatTextScene = (PackedScene)GD.Load("res://src/character/doodads/combat_text.tscn");
            CombatText combatText = (CombatText)CombatTextScene.Instance();
            caster.AddChild(combatText);
            if (mana + caster.mana > caster.manaMax)
            {
                mana = caster.manaMax - caster.mana;
            }
            combatText.SetType($"+{mana}", CombatText.TextType.MANA, caster.GetNode<Node2D>("img").Position);
            return base.Cast();
        }
    }
}