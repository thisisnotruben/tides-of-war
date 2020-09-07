using Game.Actor;
using Godot;
namespace Game.Projectile
{
	public class Missile : Node2D
	{
		private Tween tween;
		private AnimationPlayer anim;
		private Area2D hitbox;

		private Character character, target;
		private Vector2 spawnPos;
		private bool hit;

		[Signal]
		public delegate void OnHit();

		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			anim = GetNode<AnimationPlayer>("anim");
			hitbox = GetNode<Area2D>("hitbox");
			LookAt(target.pos);
		}
		private void MoveMissile()
		{
			tween.StopAll();
			tween.InterpolateProperty(this, ":global_position", GlobalPosition, target.pos,
				spawnPos.DistanceTo(target.pos) / character.stats.weaponRange.value,
				Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();
		}
		public void Init(Character character, Character target)
		{
			this.character = character;
			this.target = target;

			spawnPos = character.missileSpawnPos.GlobalPosition;
			GlobalPosition = spawnPos;
		}
		public override void _Process(float delta)
		{
			LookAt(target.pos);
			MoveMissile();
		}
		public async void OnHitBoxEntered(Area2D area2D)
		{
			if ((area2D.Owner as Character) == target && !hit)
			{
				hit = true;
				ZIndex = 1;
				CallDeferred("set", hitbox.Monitoring, false);

				EmitSignal(nameof(OnHit));

				anim.Play("missileFade");
				await ToSignal(anim, "animation_finished");

				// delete self
				SetProcess(false);
				tween.RemoveAll();
				QueueFree();
			}
		}
	}
}