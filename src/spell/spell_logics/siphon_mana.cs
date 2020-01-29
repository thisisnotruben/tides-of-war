using System;
using Game.Misc.Other;
using Godot;
namespace Game.Ability
{
    public class siphon_mana : Spell
    {
        public override float Cast()
        {
            short mana = (short)Math.Round((float)caster.GetTarget().mana * 0.2f);
            caster.GetTarget().SetMana((short) - mana);
            caster.SetMana(mana);
            PackedScene CombatTextScene = (PackedScene)GD.Load("res://src/misc/other/combat_text.tscn");
            CombatText combatText = (CombatText)CombatTextScene.Instance();
            caster.AddChild(combatText);
            if (mana + caster.mana > caster.manaMax)
            {
                mana = (short)(caster.manaMax - caster.mana);
            }
            combatText.SetType($"+{mana}", CombatText.TextType.MANA, caster.GetNode<Node2D>("img").Position);
            return base.Cast();
        }
    }
}