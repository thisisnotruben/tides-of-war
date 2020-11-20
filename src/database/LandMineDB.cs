using Godot;
using System;
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

		public static readonly LandMineDB Instance = new LandMineDB();

		public LandMineDB() : base(PathManager.landMine) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new LandMineData(
					hitBox: dict[nameof(LandMineData.hitBox)] == null ? null
						: GD.Load<Shape2D>(string.Format(PathManager.assetMissileSpellPath, (string)dict[nameof(LandMineData.hitBox)])),
					img: dict[nameof(LandMineData.img)] == null ? null
						: GD.Load<Texture>(string.Format(PathManager.assetMissileSpellPath, (string)dict[nameof(LandMineData.img)])),
					sound: (string)dict[nameof(LandMineData.sound)],
					minDamage: (int)(Single)dict[nameof(LandMineData.minDamage)],
					maxDamage: (int)(Single)dict[nameof(LandMineData.maxDamage)],
					armDelaySec: (int)(Single)dict[nameof(LandMineData.armDelaySec)],
					timeToDetSec: (int)(Single)dict[nameof(LandMineData.timeToDetSec)],
					radius: (int)(Single)dict[nameof(LandMineData.radius)]
				));
			}
		}
	}
}
