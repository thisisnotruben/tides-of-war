using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class CollNavDB
	{
		public enum Ordinal { NW, N, NE, W, E, SW, S, SE }
		public enum collNavTile : int
		{
			PASS = 1231,
			BLOCK = 1167,
			NW = 1421,
			N = 1422,
			NE_O = 1423,
			W = 1485,
			E = 1487,
			SW = 1549,
			S = 1550,
			SE_O = 1551,
			SE_I = 1419,
			SW_I = 1420,
			NE_I = 1483,
			NW_I = 1484
		}
		private static readonly collNavTile[] collNavTileID;
		private const string DB_PATH = "res://data/coll_nav.json";
		private static Dictionary<string, int[,]> graph = new Dictionary<string, int[,]>();

		static CollNavDB()
		{
			collNavTileID = new[] { collNavTile.PASS, collNavTile.BLOCK,
				collNavTile.NW, collNavTile.N, collNavTile.NE_O, collNavTile.W, collNavTile.E, collNavTile.SW, collNavTile.S, collNavTile.SE_O,
				collNavTile.SE_I, collNavTile.SW_I, collNavTile.NE_I, collNavTile.NW_I };
			LoadCollNavData();
		}
		private static void LoadCollNavData()
		{
			File file = new File();
			file.Open(DB_PATH, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();
			Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string ordinalDir in rawDict.Keys)
			{
				Godot.Collections.Array rawMatrix = (Godot.Collections.Array)rawDict[ordinalDir];
				int[,] matrix = new int[rawMatrix.Count, rawMatrix.Count];
				for (int i = 0; i < rawMatrix.Count; i++)
				{
					Godot.Collections.Array rawRow = (Godot.Collections.Array)rawMatrix[i];
					for (int j = 0; j < rawMatrix.Count; j++)
					{
						matrix[i, j] = (int)(Single)rawRow[j];
					}
				}
				graph.Add(ordinalDir, matrix);
			}
		}
		public static bool CanConnect(collNavTile from, collNavTile to, Ordinal ordinal)
		{
			return graph[ordinal.ToString()][Array.IndexOf(collNavTileID, from), Array.IndexOf(collNavTileID, to)] > 0;
		}
	}
}
