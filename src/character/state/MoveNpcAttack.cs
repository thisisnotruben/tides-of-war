using Game.Database;
namespace Game.Actor.State
{
	public class MoveNpcAttack : Move
	{
		public override void Start()
		{
			base.Start();
			if (character.target != null)
			{
				path = Map.Map.map.getAPath(character.GlobalPosition, character.target.GlobalPosition);
				MoveTo(path);
			}
		}
		public override void _OnTweenCompleted(Godot.Object Gobject, Godot.NodePath nodePath)
		{
			// TODO: add flee distance so unit doesn't keeep chasing
			if (character.target == null)
			{
				fsm.ChangeState(
					(UnitDB.GetUnitData(character.Name).path.Count > 0)
					? FSM.State.NPC_MOVE_ROAM
					: FSM.State.NPC_MOVE_RETURN);
			}
			else if (character.GetCenterPos().DistanceTo(character.target.GetCenterPos()) <= character.weaponRange)
			{
				fsm.ChangeState(FSM.State.ATTACK);
			}
			else
			{
				MoveTo(path);
			}
		}
	}
}