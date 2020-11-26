using Game.Actor;
using Godot;
using Game.Database;
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
		}
		public virtual void Init(Character character, Character target)
		{
			this.character = character;
			this.target = target;
			targetHitBox = target.hitBox;

			spawnPos = GlobalPosition = character.missileSpawnPos.GlobalPosition;

			moveBehavior = (float delta) =>
		   {
			   if (!hit)
			   {
				   LookAt(target.pos);
			   }
			   MoveMissile(GlobalPosition, target.pos);
		   };
		}
		public override void _PhysicsProcess(float delta) { moveBehavior?.Invoke(delta); }
		protected virtual void MoveMissile(Vector2 startPos, Vector2 targetPos)
		{
			tween.StopAll();
			tween.InterpolateProperty(this, ":global_position", startPos, targetPos,
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

				anim.Play("missileFade");
			}
		}
		public virtual void OnMissileFadeFinished(string animName) { Delete(); } // connected from scene
		protected void Delete()
		{
			SetPhysicsProcess(false);
			tween.RemoveAll();
			QueueFree();
		}
		public virtual Godot.Collections.Dictionary Serialize()
		{
			return new Godot.Collections.Dictionary()
			{
				{NameDB.SaveTag.CHARACTER, character?.GetPath() ?? string.Empty},
				{NameDB.SaveTag.TARGET, target?.GetPath() ?? string.Empty},
				{NameDB.SaveTag.HIT, hit},
				{NameDB.SaveTag.SPAWN_POSITION, new Godot.Collections.Array() { spawnPos.x,spawnPos.y }},
				{NameDB.SaveTag.POSITION, new Godot.Collections.Array() { GlobalPosition.x, GlobalPosition.y }}
			};
		}
		public virtual void Deserialize(Godot.Collections.Dictionary payload)
		{
			string characterPath = (string)payload[NameDB.SaveTag.CHARACTER],
				targetPath = (string)payload[NameDB.SaveTag.TARGET];

			if ((bool)payload[NameDB.SaveTag.HIT] || !HasNode(characterPath) || !HasNode(targetPath))
			{
				Delete();
			}

			Init(GetNode<Character>(characterPath), GetNode<Character>(targetPath));

			Godot.Collections.Array pos = (Godot.Collections.Array)payload[NameDB.SaveTag.SPAWN_POSITION];
			spawnPos = new Vector2((float)pos[0], (float)pos[1]);
		}
		public virtual void LoadMissile(Godot.Collections.Dictionary payload)
		{
			// TODO
			Deserialize(payload);
			Godot.Collections.Array pos = (Godot.Collections.Array)payload[NameDB.SaveTag.SPAWN_POSITION];
			GlobalPosition = new Vector2((float)pos[0], (float)pos[1]);
		}
	}
}