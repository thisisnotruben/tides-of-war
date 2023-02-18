using Godot;
using Game.Actor;
namespace Game.Quest.Custom
{
	public class q1_1407_3 : WorldQuest
	{
		public override void OnStatusChanged(QuestMaster.QuestStatus status)
		{
			base.OnStatusChanged(status);

			Character player = Player.player;
			Map.Map map = Map.Map.map;
			if (status == QuestMaster.QuestStatus.ACTIVE && player != null && map != null)
			{
				Vector2 spawnPos = map.GetGridPosition(new Vector2(304f, 624f));
				player.state = Actor.State.FSM.State.IDLE;
				map.OccupyCell(spawnPos, true);
				player.GlobalPosition = spawnPos;
			}
		}
	}
}