using System.Collections.Generic;
using Game.Actor.Doodads;
using Game.Utils;
using Godot;
namespace Game.Actor
{
    public class TargetDummy : Npc
    {
        public override void _Ready()
        {
            SetProcess(false);
        }
        public override void _Process(float delta) { }
        public override void MoveTo(Vector2 WorldPosition, List<Vector2> route) { }
        public override void Attack(bool ignoreArmor) { }
        public override void TakeDamage(int damage, bool ignoreArmor, WorldObject worldObject, CombatText.TextType textType)
        {
            base.TakeDamage(damage, ignoreArmor, worldObject, textType);
            hp = hpMax;
            mana = manaMax;
        }
        public override void SetDead(bool dead) { }
        public override void _OnSelectPressed()
        {
            if (Globals.player.target == this)
            {
                Globals.player.target = null;
                target = null;
            }
            else
            {
                Globals.PlaySound("click4", this, new Speaker());
                Sprite img = GetNode<Sprite>("img");
                Tween tween = GetNode<Tween>("tween");
                tween.InterpolateProperty(img, ":scale", img.Scale, new Vector2(1.03f, 1.03f),
                    0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
                tween.Start();
                target = Globals.player;
                Globals.player.target = this;
            }
        }
    }
}