using System.Linq;
using Game.Database;
namespace Game.Actor.State
{
	public class MoveNpcAttack : Move
	{
		public override void Start()
		{
			base.Start();
			StartPursuit();
		}
		public override void _OnTweenCompleted(Godot.Object Gobject, Godot.NodePath nodePath)
		{
			if (character.target == null || OutOfPursuitRange())
			{
				RevertState();
			}
			else if (character.pos.DistanceTo(character.target.pos) <= character.stats.weaponRange.value)
			{
				fsm.ChangeState(FSM.State.ATTACK);
			}
			else
			{
				// keep on pursuing
				MoveTo(path);
			}
		}
		private protected override void OnMoveAnomaly(MoveAnomalyType moveAnomalyType)
		{
			// just find another way to target
			StartPursuit();
		}
		private void StartPursuit()
		{
			if (character.target == null)
			{
				// can't pursuit without target
				RevertState();
			}
			else
			{
				// Get path to target and move there
				path = Map.Map.map.getAPath(character.GlobalPosition, character.target.GlobalPosition);
				MoveTo(path);
			}
		}
		private void RevertState()
		{
			fsm.ChangeState(
				(UnitDB.GetUnitData(character.Name).path.Length > 0)
				? FSM.State.NPC_MOVE_ROAM
				: FSM.State.NPC_MOVE_RETURN);
		}
		private bool OutOfPursuitRange()
		{
			if (UnitDB.GetUnitData(character.Name).path.Length > 0)
			{
				// if the closest waypoint the target is to...
				return (from waypoint in UnitDB.GetUnitData(character.Name).path
						select character.target.GlobalPosition.DistanceTo(waypoint)).Min()
						> Stats.FLEE_DISTANCE;
			}
			return character.GlobalPosition.DistanceTo(
				UnitDB.GetUnitData(character.Name).spawnPos) > Stats.FLEE_DISTANCE;
		}
	}
}