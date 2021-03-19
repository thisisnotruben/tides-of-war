using System;
using Godot;
using GC = Godot.Collections;
using Game.Database;
namespace Game.Actor.Doodads
{
	public class CombatText : Node2D, ISerializable
	{
		public enum TextType : byte { XP, GOLD, CRITICAL, MANA, DODGE, PARRY, MISS, HIT }
		private const float TIME = 1.1f, DISTANCE = 12.0f;

		private Tween tween;
		private Label label;
		public AnimationPlayer anim;

		private TextType textType;
		private Vector2 localCenterPos = Vector2.Zero;

		[Signal] public delegate void MidwayThrough();
		[Signal] public delegate void Finished();

		public override void _Ready()
		{
			label = GetChild<Label>(0);
			anim = GetChild<AnimationPlayer>(1);
			tween = GetChild<Tween>(2);
		}
		public void Init(String text, TextType textType, Vector2 localCenterPos)
		{
			label.Text = text;
			this.textType = textType;
			this.localCenterPos = localCenterPos;

			Color colorBeginning = Color.ColorN("white", 0.5f),
				colorEnd = Color.ColorN("white", 0.0f),
				selfColor = Color.ColorN("crimson");

			switch (textType)
			{
				case TextType.XP:
					selfColor = Color.ColorN("blueviolet");
					break;
				case TextType.GOLD:
					selfColor = Color.ColorN("gold");
					break;
				case TextType.MANA:
					selfColor = Color.ColorN("dodgerblue");
					break;
				case TextType.CRITICAL:
					colorBeginning = Color.ColorN("white");
					colorEnd = colorBeginning;
					break;
				case TextType.DODGE:
				case TextType.PARRY:
				case TextType.MISS:
					selfColor = Color.ColorN("white");
					break;
			}
			Modulate = selfColor;

			tween.InterpolateProperty(this, "position", localCenterPos,
				new Vector2(localCenterPos.x, localCenterPos.y - DISTANCE),
				TIME, Tween.TransitionType.Linear, Tween.EaseType.In);

			tween.InterpolateProperty(label, "modulate",
				colorBeginning, colorEnd, TIME,
				Tween.TransitionType.Linear, Tween.EaseType.In);
		}
		public void Start()
		{
			tween.Start();
			anim.Play("labelFade");
		}
		public void OnMidwayThroughAnimation() { EmitSignal(nameof(MidwayThrough)); } // called by anim
		public void OnAnimationFinished(string animName) { EmitSignal(nameof(Finished)); }
		public GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.TEXT, label.Text},
				{NameDB.SaveTag.STATE, textType.ToString()},
				{NameDB.SaveTag.POSITION, new GC.Array() { localCenterPos.x, localCenterPos.y}},
				{NameDB.SaveTag.TIME_LEFT, !anim.CurrentAnimation.Equals(string.Empty)
					? anim.CurrentAnimationPosition : 0.0f
				}
			};
		}
		public void Deserialize(GC.Dictionary payload)
		{
			float timeLeft = (float)payload[NameDB.SaveTag.TIME_LEFT];
			if (timeLeft > 0.0f)
			{
				tween.Seek(timeLeft);
				anim.Seek(timeLeft, true);
				label.Show(); // or anim will skip this
			}
		}
	}
}