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

		public AreaEffectDB(string path) : base(path) { }
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