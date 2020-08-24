using System.Collections.Generic;
using Game.Database;
using Godot;
namespace Game.Actor.State
{
	public class MoveNpcRoam : Move
	{
		private Queue<Vector2> waypoints = new Queue<Vector2>();
		private bool reversePath = false;

		public override void _Ready()
		{
			base._Ready();
			Connect(nameof(InvalidPath), this, nameof(_OnMoveAnomaly));
			Connect(nameof(ObstructionDetected), this, nameof(_OnMoveAnomaly));
		}
		public override void Start()
		{
			base.Start();
			if (waypoints.Count == 0)
			{
				// copy array
				List<Vector2> unitPath = UnitDB.GetUnitData(character.Name).path;
				unitPath = unitPath.GetRange(0, unitPath.Count);

				// in the special circumstance the unit went on to another state
				// when waypoints.Count == 0
				if (reversePath)
				{
					unitPath.Reverse();
					reversePath = false;
				}

				waypoints = new Queue<Vector2>(unitPath);
			}
			if (waypoints.Count > 0)
			{
				SetPathToWaypoint();
				MoveTo(path);
			}
		}

		// cyclic functions
		public override void _OnTweenCompleted(Object Gobject, NodePath nodePath)
		{
			SetPathToWaypoint();
			MoveTo(path);
		}
		public void _OnMoveAnomaly()
		{
			// dequeue waypoint for anomaly and focus on the other waypoint
			waypoints.Dequeue();
			if (waypoints.Count == 0)
			{
				SetWaypoints();
			}
			path = Map.Map.map.getAPath(character.GlobalPosition, waypoints.Peek());
			MoveTo(path);
		}

		// helper functions
		private void SetPathToWaypoint()
		{
			// get next waypoint
			if (path.Count == 0)
			{
				// dequeue waypoint: we're already in that tile
				waypoints.Dequeue();
				if (waypoints.Count == 0)
				{
					SetWaypoints();
				}
				// get path to waypoint
				path = Map.Map.map.getAPath(character.GlobalPosition, waypoints.Peek());
			}
		}
		private void SetWaypoints()
		{
			// copy array
			List<Vector2> mapPatrolPath = UnitDB.GetUnitData(character.Name).path;
			mapPatrolPath = mapPatrolPath.GetRange(0, mapPatrolPath.Count);

			reversePath = !reversePath;
			if (reversePath)
			{
				mapPatrolPath.Reverse();
			}

			waypoints = new Queue<Vector2>(mapPatrolPath);
			waypoints.Dequeue();
		}
	}
}