using Godot;
using Game.Actor;

namespace Game.Spell
{
    public class Volley : Spell
    {
        private Character target = null;
        private const float animSpeed = 1.5f;
        private const string animName = "attacking";

        public override float Cast()
        {
            // TODO: not sure how this class is going to work, need to test
            return base.Cast();
        }
        public void VolleyCast(string animName)
        {
            AnimationPlayer casterAnim = caster.GetNode<AnimationPlayer>("anim");

            if (target == null)
            {
                if (!loaded)
                {
                    caster.mana -= manaCost;
                }
                SetCount(3);
                target = caster.GetTarget();
                casterAnim.Play(animName, -1, animSpeed);
            }
            else
            {
                if (caster.GetCenterPos().DistanceTo(target.GetCenterPos()) <= caster.weaponRange)
                {
                    count--;
                    if (count > 0)
                    {
                        casterAnim.Play(animName, -1, animSpeed);
                    }
                }
                else if (count < 0)
                {
                    caster.GetNode<Timer>("timer").SetBlockSignals(false);        
                    caster.SetProcess(true);
                    UnMake();
                }
            }
        }
        public override void ConfigureSpell()
        {
            caster.SetProcess(false);
            caster.GetNode<Timer>("timer").SetBlockSignals(true);
            AnimationPlayer casterAnim = caster.GetNode<AnimationPlayer>("anim");
            casterAnim.Connect("animation_finished", this, nameof(VolleyCast));
            casterAnim.Play(animName, -1.0f, animSpeed);
        }
    }
}
