using System;
using System.Collections.Generic;
using Game.Actor;
using Godot;
namespace Game.Spell
{
    public class Meteor : OsbSpell
    {
        private List<Character> targetList = new List<Character>();
        public override void Init(string worldName)
        {
            base.Init(worldName);
            float spellRadius = 24.0f;
            TouchScreenButton spellArea = GetNode<TouchScreenButton>("spell_area");
            spellArea.SetScale(new Vector2(spellRadius, spellRadius) * 2.0f / spellArea.GetTexture().GetSize());
            spellArea.SetPosition(spellArea.GetTexture().GetSize() * spellArea.GetScale() / -2.0f);
            ((CircleShape2D)GetNode<CollisionShape2D>("sight/distance").GetShape()).SetRadius(spellRadius);
        }
        public override float Cast()
        {
            // TODO: not sure how this method will work
            return base.Cast();
        }
        public void MeteorCast(bool hit = false)
        {
            if (hit)
            {
                GD.Randomize();
                short damage = (short)(Math.Round(GD.RandRange(caster.minDamage, caster.maxDamage) * 1.2f));
                foreach (Character character in targetList)
                {
                    character.TakeDamage(damage, false, caster, Game.Misc.Other.CombatText.TextType.HIT);
                }
                UnMake();
            }
            else
            {
                Transform2D ctrans = GetCanvasTransform();
                Vector2 minPos = -ctrans.origin / ctrans.Scale;
                Vector2 maxPos = minPos + GetViewportRect().Size / ctrans.Scale;
                PackedScene meteorScene = (PackedScene)GD.Load("res://src/spell/spellEffects/meteor.tscn");
                MeteorEffect meteorEffect = (MeteorEffect)meteorScene.Instance();
                meteorEffect.seekPos = GetGlobalPosition();
                float side = (meteorEffect.seekPos.x > 0.50f * (maxPos.x - minPos.x) + minPos.x) ? 0.25f : 0.75f;
                meteorEffect.SetGlobalPosition(new Vector2(side * (maxPos.x - minPos.x) + minPos.x, minPos.y));
                meteorEffect.Connect(nameof(MeteorEffect.Hit), this, nameof(MeteorCast));
                caster.GetParent().AddChild(meteorEffect);
                meteorEffect.SetOwner(caster.GetParent());
            }
        }
    }
}