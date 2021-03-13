using Godot;
namespace Game.Database
{
	public class LandMineDB : AbstractDB<LandMineDB.LandMineData>
	{
		public class LandMineData
		{
			public readonly Shape2D hitBox;
			public readonly Texture img;
			public readonly string sound;
			public readonly int minDamage, maxDamage, armDelaySec, timeToDetSec, radius;

			public LandMineData(Shape2D hitBox, Texture img, string sound,
			int minDamage, int maxDamage, int armDelaySec, int timeToDetSec, int radius)
			{
				this.hitBox = hitBox;
				this.img = img;
				this.sound = sound;
				this.minDamage = minDamage;
				this.maxDamage = maxDamage;
				this.armDelaySec = armDelaySec;
				this.timeToDetSec = timeToDetSec;
				this.radius = radius;
			}
		}

		public LandMineDB(string path) : base(path) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new LandMineData(
					hitBox: dict[nameof(LandMineData.hitBox)] == null ? null
						: GD.Load<Shape2D>(string.Format(PathManager.assetMissileSpellPath, dict[nameof(LandMineData.hitBox)])),
					img: dict[nameof(LandMineData.img)] == null ? null
						: GD.Load<Texture>(string.Format(PathManager.assetMissileSpellPath, dict[nameof(LandMineData.img)])),
					sound: dict[nameof(LandMineData.sound)].ToString(),
					minDamage: dict[nameof(LandMineData.minDamage)].ToString().ToInt(),
					maxDamage: dict[nameof(LandMineData.maxDamage)].ToString().ToInt(),
					armDelaySec: dict[nameof(LandMineData.armDelaySec)].ToString().ToInt(),
					timeToDetSec: dict[nameof(LandMineData.timeToDetSec)].ToString().ToInt(),
					radius: dict[nameof(LandMineData.radius)].ToString().ToInt()
				));
			}
		}
	}
}
