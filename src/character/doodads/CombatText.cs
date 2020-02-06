using System;
using Godot;
namespace Game.Actor.Doodads
{
    public class CombatText : Node2D
    {
        public enum TextType : byte { XP, GOLD, CRITICAL, MANA, DODGE, PARRY, MISS, HIT }
        private const float TIME = 1.1f;
        private Tween tween;
        private Label label;
        private AnimationPlayer anim;
        [Signal]
        public delegate void QueueStart();
        public override void _Ready()
        {
            tween = GetNode<Tween>("tween");
            label = GetNode<Label>("label");
            anim = GetNode<AnimationPlayer>("anim");
        }
        public void SetType(String text, TextType textType, Vector2 localCenterPos)
        {
            label.Text = text;
            localCenterPos.x = -label.RectSize.x / 2.0f;
            localCenterPos.y -= 8.0f;
            Color colorBeginning = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            Color colorEnd = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            Color selfColor = new Color("ff0000");
            switch (textType)
            {
                case TextType.XP:
                    selfColor = new Color("800080");
                    break;
                case TextType.GOLD:
                    selfColor = new Color("ffff00");
                    break;
                case TextType.MANA:
                    selfColor = new Color("00afff");
                    break;
                case TextType.CRITICAL:
                    colorBeginning = new Color("ffffff");
                    colorEnd = colorBeginning;
                    break;
                case TextType.DODGE:
                case TextType.PARRY:
                case TextType.MISS:
                    selfColor = new Color("ffffff");
                    break;
            }
            Modulate = selfColor;
            tween.InterpolateProperty(this, ":position", localCenterPos,
                new Vector2(localCenterPos.x, localCenterPos.y - 14.0f), TIME, Tween.TransitionType.Linear, Tween.EaseType.In);
            tween.InterpolateProperty(label, ":modulate", colorBeginning, colorEnd, TIME, Tween.TransitionType.Linear, Tween.EaseType.In);
            CombatText combatText = GetParent().GetChild(GetIndex() - 1)as CombatText;
            if (combatText != null)
            {
                combatText.Connect(nameof(QueueStart), this, nameof(Start));
            }
            else
            {
                Start();
            }
        }
        public void Queue()
        {
            EmitSignal(nameof(QueueStart));
        }
        public void Start()
        {
            if (!tween.IsActive())
            {
                tween.Start();
                anim.Play("label_fade");
            }
        }
        public void _OnAnimFinished(string animName)
        {
            EmitSignal(nameof(QueueStart));
            QueueFree();
        }
    }
}