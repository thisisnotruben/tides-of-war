using GC = Godot.Collections;
namespace Game.Actor.State
{
	public class Idle : TakeDamage
	{
		public override void Start()
		{
			character.anim.Stop();
			character.img.Frame = 0;
			character.img.FlipH = false;

			Map.Map.map.OccupyCell(character.GlobalPosition, true);

			// for player to check surrounding areas
			if (character.target?.enemy ?? false)
			{
				(character as Player)?.OnAttacked(character.target);
			}
		}
		public override void Exit() { Map.Map.map.OccupyCell(character.GlobalPosition, false); }
		public override GC.Dictionary Serialize() { return new GC.Dictionary(); }
	}
}