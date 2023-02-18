using Game.Actor;
using Godot;
namespace Game.Projectile
{
	public class MissileSpellOrbital : MissileSpell
	{
		public const float SPEED = 40.0f;

		public override Missile Init(Character character, Character target)
		{
			this.character = character;
			this.target = target;
			targetHitBox = target.hitBox;

			Map.Map.map.AddZChild(this);
			Owner = Map.Map.map;

			Transform2D ctrans = character.GetCanvasTransform();
			Vector2 minPos = -ctrans.origin / ctrans.Scale,
					maxPos = minPos + character.GetViewportRect().Size / ctrans.Scale;
			float screenSide = target.pos.x > 0.50f * (maxPos.x - minPos.x) + minPos.x ? 0.25f : 0.75f;

			spawnPos = new Vector2(screenSide * (maxPos.x - minPos.x) + minPos.x, minPos.y);

			moveBehavior = (float delta) => GlobalPosition +=
				GlobalPosition.DirectionTo(target.pos) * SPEED * delta;

			return this;
		}
		public override void _Ready()
		{
			base._Ready();
			GlobalPosition = spawnPos;
		}
	}
}