using Game.Actor;
using Game.Database;
using Godot;
namespace Game.Map.Doodads
{
	public class InteractItemScout : Area2D
	{
		private string scoutValue = string.Empty;

		public override void _Ready()
		{
			Connect("area_entered", this, nameof(OnPlayerEntered));
		}
		public void Activate(string scoutValue, bool activate)
		{
			this.scoutValue = activate ? scoutValue : string.Empty;
			Monitoring = activate;
		}
		private void OnPlayerEntered(Area2D area2D)
		{
			Player player = area2D.Owner as Player;
			if (player != null && !player.dead && !scoutValue.Empty())
			{
				Globals.questMaster.CheckQuests(scoutValue, QuestDB.QuestType.SCOUT, true);
			}
		}
	}
}