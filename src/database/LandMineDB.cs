using Godot;
using System;
using System.Collections.Generic;
namespace Game.Database
{
	public static class LandMineDB
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

		private static Dictionary<string, LandMineData> landMineData;

		public static void Init() { landMineData = LoadLandMineData(PathManager.landMine); }
		private static Dictionary<string, LandMineData> LoadLandMineData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, LandMineData> landMineData = new Dictionary<string, LandMineData>();

			string filePath = "res://asset/img/missile-spell/{0}.tres";
			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				landMineData.Add(worldName, new LandMineData(
					hitBox: dict[nameof(LandMineData.hitBox)] == null ? null
						: GD.Load<Shape2D>(string.Format(filePath, (string)dict[nameof(LandMineData.hitBox)])),
					img: dict[nameof(LandMineData.img)] == null ? null
						: GD.Load<Texture>(string.Format(filePath, (string)dict[nameof(LandMineData.img)])),
					sound: (string)dict[nameof(LandMineData.sound)],
					minDamage: (int)(Single)dict[nameof(LandMineData.minDamage)],
					maxDamage: (int)(Single)dict[nameof(LandMineData.maxDamage)],
					armDelaySec: (int)(Single)dict[nameof(LandMineData.armDelaySec)],
					timeToDetSec: (int)(Single)dict[nameof(LandMineData.timeToDetSec)],
					radius: (int)(Single)dict[nameof(LandMineData.radius)]
				));
			}

			return landMineData;
		}
		public static bool HasLandMineData(string worldName) { return landMineData.ContainsKey(worldName); }
		public static LandMineData GetLandMineData(string worldName) { return landMineData[worldName]; }
	}
}
