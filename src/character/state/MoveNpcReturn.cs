using Game.Database;
namespace Game.Actor.State
{
	public class MoveNpcReturn : Move
	{
		public override void Start()
		{
			base.Start();
			if (UnitDB.HasUnitData(character.Name))
			{
				path = Map.Map.map.getAPath(character.GlobalPosition, UnitDB.GetUnitData(character.Name).spawnPos);
				MoveTo(path);
			}
			else
			{
				fsm.ChangeState(FSM.State.IDLE);
			}
		}
	}
}