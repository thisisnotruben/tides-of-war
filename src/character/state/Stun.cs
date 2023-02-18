using GC = Godot.Collections;
namespace Game.Actor.State
{
	public class Stun : TakeDamage
	{
		public override void Start() { Map.Map.map.OccupyCell(character.GlobalPosition, true); }
		public override void Exit() { Map.Map.map.OccupyCell(character.GlobalPosition, false); }
		public override void OnAttacked(Character whosAttacking)
		{
			ClearOnAttackedSignals(whosAttacking);
			character.regenTimer.Stop();
		}
		public override GC.Dictionary Serialize() { return new GC.Dictionary(); }
	}
}