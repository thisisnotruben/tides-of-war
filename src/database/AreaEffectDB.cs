using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class AreaEffectDB
	{
		public struct AreaEffectNode { public int radius; }

		private static readonly Dictionary<string, AreaEffectNode> areaEffectData;

		static AreaEffectDB()
		{
			areaEffectData = LoadAreaEffectData("res://data/areaEffect.json");
		}
		public static void Init() { }
		private static Dictionary<string, AreaEffectNode> LoadAreaEffectData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, AreaEffectNode> areaEffectData = new Dictionary<string, AreaEffectNode>();

			Godot.Collections.Dictionary dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string worldName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[worldName];

				AreaEffectNode areaEffectNode;
				areaEffectNode.radius = (int)((Single)dict[nameof(AreaEffectNode.radius)]);

				areaEffectData.Add(worldName, areaEffectNode);
			}

			return areaEffectData;
		}
		public static bool HasAreaEffect(string worldName) { return areaEffectData.ContainsKey(worldName); }
		public static AreaEffectNode GetAreaEffect(string worldName) { return areaEffectData[worldName]; }
	}
}