using Game.Database;
using Game.Mine;
namespace Game.Ability
{
	public class ExplosiveTrap : Spell
	{
		public override void Start()
		{
			base.Start();

			LandMineDB.LandMineData landMineData = LandMineDB.GetLandMineData(worldName);

			LandMine landMine = (LandMine)SceneDB.landMine.Instance();
			landMine.Init(character, landMineData.maxDamage, landMineData.maxDamage,
				landMineData.armDelaySec, landMineData.timeToDetSec);
		}
	}
}