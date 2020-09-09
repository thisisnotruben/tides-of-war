using System;
using Godot;
namespace Game.Actor.Doodads
{
	public class CombatText : Node2D
	{
		public static readonly PackedScene scene = (PackedScene)GD.Load("res://src/character/doodads/CombatText.tscn");

		public enum TextType : byte { XP, GOLD, CRITICAL, MANA, DODGE, PARRY, MISS, HIT }
		private const float TIME = 1.1f, DISTANCE = 12.0f;

		private Tween tween;
		private Label label;
		public AnimationPlayer anim;

		[Signal]
		public delegate void MidwayThrough();
		[Signal]
		public delegate void Finished();

		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			label = GetNode<Label>("label");
			anim = GetNode<AnimationPlayer>("anim");
		}
		public void Init(String text, TextType textType, Vector2 localCenterPos)
		{
			label.Text = text;

			Color colorBeginning = new Color("80ffffff"),
				colorEnd = new Color("00ffffff"),
				selfColor = new Color("ff0000");

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
				new Vector2(localCenterPos.x, localCenterPos.y - DISTANCE),
				TIME, Tween.TransitionType.Linear, Tween.EaseType.In);

			tween.InterpolateProperty(label, ":modulate",
				colorBeginning, colorEnd, TIME,
				Tween.TransitionType.Linear, Tween.EaseType.In);
		}
		public void Start()
		{
			tween.Start();
			anim.Play("labelFade");
		}
		public void OnMidwayThroughAnimation() { EmitSignal(nameof(MidwayThrough)); }
		public void OnAnimationFinished(string animName) { EmitSignal(nameof(Finished)); }
	}
}