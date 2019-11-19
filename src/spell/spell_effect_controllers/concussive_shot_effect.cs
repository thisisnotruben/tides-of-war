using Godot;
namespace Game.Ability
{
    public class ConcussiveShotEffect : SpellEffect
    {
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            GetNode<Node2D>("light").Show();
            PackedScene bashScene = (PackedScene)GD.Load("res://src/spell/effects/bash.tscn");
            BashEffect bash = (BashEffect)bashScene.Instance();
            character.GetTarget().AddChild(bash);
            bash.SetOwner(character.GetTarget());
            spell.Connect(nameof(Unmake), bash, nameof(BashEffect._OnTimerTimeout));
            bash.GetNode<AudioStreamPlayer2D>("snd").SetStream(null);
            bash.OnHit();
            tween.Start();
            timer.Start();
        }
        public override void _OnTimerTimeout()
        {
            base._OnTimerTimeout();
            QueueFree();
        }
    }
}