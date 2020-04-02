using System;
using System.Collections.Generic;
using Game.Actor;
using Godot;
namespace Game.Ability
{
    public class meteor : OsbSpell
    {
        private List<Character> targetList = new List<Character>();
        public override void Init(string worldName)
        {
            base.Init(worldName);
            float spellRadius = 24.0f;
            TouchScreenButton spellArea = GetNode<TouchScreenButton>("spell_area");
            spellArea.Scale = (new Vector2(spellRadius, spellRadius) * 2.0f / spellArea.Normal.GetSize());
            spellArea.Position = spellArea.Normal.GetSize() * spellArea.Scale / -2.0f;
            ((CircleShape2D)GetNode<CollisionShape2D>("sight/distance").Shape).Radius = spellRadius;
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
                int damage = (int)(Math.Round(GD.RandRange(caster.minDamage, caster.maxDamage) * 1.2f));
                foreach (Character character in targetList)
                {
                    character.TakeDamage(damage, false, caster, Game.Actor.Doodads.CombatText.TextType.HIT);
                }
                UnMake();
            }
            else
            {
                Transform2D ctrans = GetCanvasTransform();
                Vector2 minPos = -ctrans.origin / ctrans.Scale;
                Vector2 maxPos = minPos + GetViewportRect().Size / ctrans.Scale;
                PackedScene meteorScene = (PackedScene)GD.Load("res://src/spell/spellEffects/meteor.tscn");
                meteor_effect meteorEffect = (meteor_effect)meteorScene.Instance();
                meteorEffect.seekPos = GlobalPosition;
                float side = (meteorEffect.seekPos.x > 0.50f * (maxPos.x - minPos.x) + minPos.x) ? 0.25f : 0.75f;
                meteorEffect.GlobalPosition = new Vector2(side * (maxPos.x - minPos.x) + minPos.x, minPos.y);
                meteorEffect.Connect(nameof(meteor_effect.Hit), this, nameof(MeteorCast));
                caster.GetParent().AddChild(meteorEffect);
                meteorEffect.Owner = caster.GetParent();
            }
        }
    }
}