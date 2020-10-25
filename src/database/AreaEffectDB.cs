using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class AreaEffectDB
	{
		public class AreaEffectData
		{
			public readonly int radius;

			public AreaEffectData(int radius)
			{
				this.radius = radius;
			}
		}

		private static readonly Dictionary<string, AreaEffectData> areaEffectData;

		static AreaEffectDB()
		{
			areaEffectData = LoadAreaEffectData("res://data/areaEffect.json");
		}
		public static void Init() { }
		private static Dictionary<string, AreaEffectData> LoadAreaEffectData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, AreaEffectData> areaEffectData = new Dictionary<string, AreaEffectData>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				areaEffectData.Add(worldName, new AreaEffectData(
					radius: (int)((Single)dict[nameof(AreaEffectData.radius)]))
				);
			}

			return areaEffectData;
		}
		public static bool HasAreaEffect(string worldName) { return areaEffectData.ContainsKey(worldName); }
		public static AreaEffectData GetAreaEffect(string worldName) { return areaEffectData[worldName]; }
	}
}