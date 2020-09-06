using Game.Database;
namespace Game.Actor.State
{
	public class MoveNpcReturn : Move
	{
		public override void Start()
		{
			base.Start();

			GetReturnPath();
			if (path.Count > 0 && path[0].Equals(UnitDB.GetUnitData(character.Name).spawnPos))
			{
				fsm.ChangeState(FSM.State.IDLE);
			}
			else
			{
				MoveTo(path);
			}
		}
		private protected override void OnMoveAnomaly(MoveAnomalyType moveAnomalyType)
		{
			switch (moveAnomalyType)
			{
				case MoveAnomalyType.INVALID_PATH:
					// happens that npc tries go to spanwPos 
					// when already at spawnPos; just in case
					fsm.ChangeState(FSM.State.IDLE);
					break;
				case MoveAnomalyType.OBSTRUCTION_DETECTED:
					GetReturnPath();
					break;
			}
		}
		private void GetReturnPath()
		{
			path = Map.Map.map.getAPath(character.GlobalPosition, UnitDB.GetUnitData(character.Name).spawnPos);
		}
	}
}