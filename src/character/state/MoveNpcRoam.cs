using System.Collections.Generic;
using System;
using Godot;
using GC = Godot.Collections;
using Game.Database;
namespace Game.Actor.State
{
	public class MoveNpcRoam : Move
	{
		public Queue<Vector2> waypoints = new Queue<Vector2>();
		private bool reversePath = false;

		public override void Start()
		{
			base.Start();
			if (waypoints.Count == 0)
			{
				Vector2[] unitPath = Globals.unitDB.GetData(character.Name).path;

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
			else
			{
				fsm.ChangeState(FSM.State.NPC_MOVE_RETURN);
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
			Vector2[] mapPatrolPath = Globals.unitDB.GetData(character.Name).path;

			reversePath = !reversePath;
			if (reversePath)
			{
				Array.Reverse(mapPatrolPath);
			}

			waypoints = new Queue<Vector2>(mapPatrolPath);
			waypoints.Dequeue();
		}
		public override GC.Dictionary Serialize()
		{
			GC.Dictionary payload = base.Serialize();
			payload[NameDB.SaveTag.REVERSE] = reversePath;

			GC.Array wayPointArr = new GC.Array();
			foreach (Vector2 point in waypoints.ToArray())
			{
				wayPointArr.Add(point.x);
				wayPointArr.Add(point.y);
			}
			payload[NameDB.SaveTag.WAY_POINTS] = wayPointArr;

			return payload;
		}
		public override void Deserialize(GC.Dictionary payload)
		{
			base.Deserialize(payload);
			reversePath = (bool)payload[NameDB.SaveTag.REVERSE];
		}
	}
}