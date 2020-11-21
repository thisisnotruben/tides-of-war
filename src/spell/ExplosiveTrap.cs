using Game.Database;
using Game.Mine;
namespace Game.Ability
{
	public class ExplosiveTrap : Spell
	{
		public override void Start()
		{
			base.Start();

			LandMineDB.LandMineData landMineData = LandMineDB.Instance.GetData(worldName);

			LandMine landMine = (LandMine)SceneDB.landMine.Instance();
			landMine.Init(worldName, character);
		}
	}
}