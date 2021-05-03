using Game.Database;
using Game.Mine;
namespace Game.Ability
{
	public class ExplosiveTrap : Spell
	{
		public override void Start()
		{
			base.Start();
			SceneDB.landMine.Instance<LandMine>().Init(worldName, character);
		}
	}
}