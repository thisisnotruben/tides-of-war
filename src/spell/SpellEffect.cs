using Godot;
using Game.Actor;
using System;

namespace Game.Spell
{
    public abstract class SpellEffect : WorldObject
    {
        private protected Vector2 seekPos = new Vector2();
        private protected float lightFadeDelay = 0.65f;
        private protected bool MissileplaySound = false;
        private protected bool fadeLight = true;
        private protected Character character;
        private protected Tween tween;
        private protected Timer timer;

        public override void _Ready()
        {
            SetWorldType((WorldTypes)Enum.Parse(typeof(WorldTypes), GetWorldName().ToUpper().Replace("", "_")));
            foreach (Node2D node2D in GetNode("idle").GetChildren())
            {
                node2D.SetUseParentMaterial(true);
            }
            foreach (Node2D node2D in GetNode("explode").GetChildren())
            {
                node2D.SetUseParentMaterial(true);
            }
            tween = GetNode<Tween>("tween");
            timer = GetNode<Timer>("timer");
            SetProcess(false);
        }        
        public override void _Process(float delta)
        {
            Tween tween = GetNode<Tween>("tween");
            tween.InterpolateProperty(this, ":global_position", GetGlobalPosition(),
                seekPos, 5.0f, Tween.TransitionType.Circ, Tween.EaseType.Out);
            tween.Start();
            if (GetGlobalPosition().DistanceTo(seekPos) < 2.0f)
            {
                SetProcess(false);
                OnHit();
            }
        }
        public void FadeLight(bool fade = true)
        {
            if (fade)
            {
                Tween tween = GetNode<Tween>("tween");
                Node2D light = GetNode<Node2D>("light");
                tween.InterpolateProperty(light, ":modulate", light.GetModulate(),
                    new Color(1.0f, 1.0f, 1.0f, 0.0f), lightFadeDelay, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            }
        }   
        public virtual void _OnTimerTimeout()
        {
            EmitSignal(nameof(Unmake));
        }
        public virtual void OnHit(Spell spell = null)
        {
            GetNode<AudioStreamPlayer2D>("snd").Play();
            tween.InterpolateProperty(this, ":scale", new Vector2(0.75f, 0.75f),
                new Vector2(1.0f, 1.0f), 0.5f, Tween.TransitionType.Elastic, Tween.EaseType.Out);
            FadeLight(fadeLight);
            foreach (Particles2D particles2D in GetNode("idle").GetChildren())
            {
                particles2D.SetEmitting(false);
            }
            foreach (Particles2D particles2D in GetNode("explode").GetChildren())
            {
                particles2D.SetEmitting(true);
            }
            // Call this method first in every polymorphed method
        }
    }
}