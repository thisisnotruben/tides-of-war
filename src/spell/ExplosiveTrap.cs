using Game.Database;
using Game.Mine;
namespace Game.Ability
{
	public class ExplosiveTrap : Spell
	{
		public override void Start()
		{
			base.Start();
			((LandMine)SceneDB.landMine.Instance()).Init(worldName, character);
		}
	}
}