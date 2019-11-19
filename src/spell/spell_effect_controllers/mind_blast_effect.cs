using Game.Misc.Missile;
using Godot;
namespace Game.Ability
{
    public class MindBlastEffect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            Bolt bolt = GetOwner()as Bolt;
            if (bolt != null)
            {
                bolt.SetGlobalPosition(bolt.GetTarget().GetNode<Node2D>("head").GetGlobalPosition());
                tween.Start();
                timer.Start();
            }
            else
            {
                GD.Print("Owner not bolt in class MindBlast");
            }
        }
        public override void _OnTimerTimeout()
        {
            base._OnTimerTimeout();
            QueueFree();
        }
    }
}