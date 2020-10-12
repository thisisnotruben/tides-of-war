using System.Collections.Generic;
using System;
using Game.Database;
using Godot;
namespace Game.Actor.State
{
	public class MoveNpcRoam : Move
	{
		private Queue<Vector2> waypoints = new Queue<Vector2>();
		private bool reversePath = false;

		public override void Start()
		{
			base.Start();
			if (waypoints.Count == 0)
			{
				Vector2[] unitPath = UnitDB.GetUnitData(character.Name).path;

				// in the special circumstance the unit went on to another state
				// when waypoints.Count == 0
				if (reversePath)
				{
					Array.Reverse(unitPath);
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
		protected override void OnMovePointFinished()
		{
			SetPathToWaypoint();
			MoveTo(path);
		}
		protected override void OnMoveAnomaly(MoveAnomalyType moveAnomalyType)
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
			Vector2[] mapPatrolPath = UnitDB.GetUnitData(character.Name).path;

			reversePath = !reversePath;
			if (reversePath)
			{
				Array.Reverse(mapPatrolPath);
			}

			waypoints = new Queue<Vector2>(mapPatrolPath);
			waypoints.Dequeue();
		}
	}
}