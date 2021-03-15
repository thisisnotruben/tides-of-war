using Game.Actor;
using Godot;
using Game.Database;
using GC = Godot.Collections;
namespace Game.Projectile
{
	public class Missile : Node2D, ISerializable
	{
		protected Tween tween;
		protected AnimationPlayer anim;
		protected Sprite img;
		protected Area2D hitbox, targetHitBox;
		protected CollisionShape2D hitboxBody;

		protected Character character, target;
		protected Vector2 spawnPos;
		protected bool hit;
		protected const string ANIM_NAME = "missileFade";

		[Signal] public delegate void OnHit();
		protected delegate void MoveBehavior(float delta);
		protected MoveBehavior moveBehavior, moveMissile;

		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			anim = GetNode<AnimationPlayer>("anim");
			img = GetNode<Sprite>("img");
			hitbox = GetNode<Area2D>("hitbox");
			hitboxBody = hitbox.GetNode<CollisionShape2D>("body");

			AddToGroup(Globals.SAVE_GROUP);
		}
		public virtual Missile Init(Character character, Character target)
		{
			this.character = character;
			this.target = target;
			targetHitBox = target.hitBox;

			Map.Map.map.AddZChild(this);
			Owner = Map.Map.map;
			spawnPos = GlobalPosition = character.missileSpawnPos.GlobalPosition;
			LookAt(target.pos);

			moveBehavior = (float delta) =>
		   {
			   if (!hit)
			   {
				   LookAt(target.pos);
			   }
			   MoveMissile(GlobalPosition, target.pos);
		   };
			return this;
		}
		public override void _PhysicsProcess(float delta) { moveBehavior?.Invoke(delta); }
		protected virtual void MoveMissile(Vector2 startPos, Vector2 targetPos)
		{
			tween.StopAll();
			tween.InterpolateProperty(this, "global_position", startPos, targetPos,
				spawnPos.DistanceTo(targetPos) / character.stats.weaponRange.value,
				Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();
		}
		public void OnHitBoxEntered(Area2D area2D)
		{
			if (!hit && area2D == targetHitBox)
			{
				hit = true;
				ZIndex = 1;

				EmitSignal(nameof(OnHit));

				anim.Play(ANIM_NAME);
			}
		}
		public virtual void OnMissileFadeFinished(string animName) { Delete(); } // connected from scene
		protected void Delete()
		{
			SetPhysicsProcess(false);
			tween.RemoveAll();
			QueueFree();
		}
		public virtual GC.Dictionary Serialize()
		{
			return new GC.Dictionary()
			{
				{NameDB.SaveTag.CHARACTER, (character?.GetPath() ?? string.Empty).ToString()},
				{NameDB.SaveTag.TARGET, (target?.GetPath() ?? string.Empty).ToString()},
				{NameDB.SaveTag.HIT, hit},
				{NameDB.SaveTag.SPAWN_POSITION, new GC.Array() { spawnPos.x,spawnPos.y }},
				{NameDB.SaveTag.POSITION, new GC.Array() { GlobalPosition.x, GlobalPosition.y }},
				{NameDB.SaveTag.ANIM_POSITION, !anim.CurrentAnimation.Equals(string.Empty) ? anim.CurrentAnimationPosition : 0.0f},
				{NameDB.SaveTag.SPELL, string.Empty} // for missile factory
			};
		}
		public virtual void Deserialize(GC.Dictionary payload)
		{
			hit = (bool)payload[NameDB.SaveTag.HIT];

			GC.Array pos = (GC.Array)payload[NameDB.SaveTag.SPAWN_POSITION];
			spawnPos = new Vector2((float)pos[0], (float)pos[1]);

			pos = (GC.Array)payload[NameDB.SaveTag.POSITION];
			GlobalPosition = new Vector2((float)pos[0], (float)pos[1]);

			if (hit)
			{
				float animLeft = (float)payload[NameDB.SaveTag.ANIM_POSITION];
				if (animLeft > 0.0f)
				{
					anim.Play(ANIM_NAME);
					anim.Seek(animLeft);
				}
				else
				{
					Delete();
				}
			}
			else
			{
				character.fsm.ConnectMissileHit(this, character, target);
			}
		}
	}
}