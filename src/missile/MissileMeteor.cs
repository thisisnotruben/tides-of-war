using Godot;
namespace Game.Projectile
{
	public class MissileMeteor : MissileSpell
	{
		public new static PackedScene scene = (PackedScene)GD.Load("res://src/projectile/MissileMeteor.cs");

		public void Init(Area2D targetHitBox, string spellWorldName)
		{
			this.targetHitBox = targetHitBox;
			this.spellWorldName = spellWorldName;

			moveBehavior = () =>
				MoveMissile(GlobalPosition, targetHitBox.GlobalPosition);

			InstanceSpellEffect(null);
		}
		protected override void MoveMissile(Vector2 startPos, Vector2 targetPos)
		{

		}
	}
}