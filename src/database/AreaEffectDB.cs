using System;
namespace Game.Database
{
	public class AreaEffectDB : AbstractDB<AreaEffectDB.AreaEffectData>
	{
		public class AreaEffectData
		{
			public readonly int radius;

			public AreaEffectData(int radius)
			{
				this.radius = radius;
			}
		}

		public static readonly AreaEffectDB Instance = new AreaEffectDB();

		public AreaEffectDB() : base(PathManager.areaEffect) { }
		public override void LoadData(string path)
		{
			Godot.Collections.Dictionary dict, rawDict = LoadJson(path);
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				data.Add(worldName, new AreaEffectData(
					radius: (int)(Single)dict[nameof(AreaEffectData.radius)]
				));
			}
		}
	}
}