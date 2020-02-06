using Godot;
namespace Game.Ability
{
    public class concussive_shot_effect : SpellEffect
    {
        public override void Init(Actor.Character character)
        {
            base.Init(character);
            lightFadeDelay = 0.75f;
            playSound = true;
        }
        public override void OnHit(Spell spell = null)
        {
            base.OnHit(spell);
            GetNode<Node2D>("light").Show();
            PackedScene bashScene = (PackedScene)GD.Load("res://src/spell/effects/bash.tscn");
            bash_effect bash = (bash_effect)bashScene.Instance();
            character.target.AddChild(bash);
            bash.Owner = character.target;
            spell.Connect(nameof(Unmake), bash, nameof(bash_effect._OnTimerTimeout));
            bash.GetNode<AudioStreamPlayer2D>("snd").Stream = null;
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