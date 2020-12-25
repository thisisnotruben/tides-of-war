using System.Collections.Generic;
using System;
using Godot;
using GC = Godot.Collections;
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
		private static Dictionary<Ordinal, int[,]> graph = LoadCollNavData(PathManager.collNav);

		static CollNavDB()
		{
			collNavTileID = new[] { collNavTile.PASS, collNavTile.BLOCK,
				collNavTile.NW, collNavTile.N, collNavTile.NE_O, collNavTile.W, collNavTile.E, collNavTile.SW, collNavTile.S, collNavTile.SE_O,
				collNavTile.SE_I, collNavTile.SW_I, collNavTile.NE_I, collNavTile.NW_I };
		}
		private static Dictionary<Ordinal, int[,]> LoadCollNavData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<Ordinal, int[,]> graph = new Dictionary<Ordinal, int[,]>();

			GC.Dictionary rawDict = (GC.Dictionary)jSONParseResult.Result;

			int i, j;
			int[,] matrix;
			GC.Array rawMatrix, rawRow;
			foreach (string ordinalDir in rawDict.Keys)
			{
				rawMatrix = (GC.Array)rawDict[ordinalDir];
				matrix = new int[rawMatrix.Count, rawMatrix.Count];
				for (i = 0; i < rawMatrix.Count; i++)
				{
					rawRow = (GC.Array)rawMatrix[i];
					for (j = 0; j < rawMatrix.Count; j++)
					{
						matrix[i, j] = (int)(Single)rawRow[j];
					}
				}
				graph.Add((Ordinal)Enum.Parse(typeof(Ordinal), ordinalDir), matrix);
			}
			return graph;
		}
		public static bool CanConnect(collNavTile from, collNavTile to, Ordinal ordinal)
		{
			return graph[ordinal][Array.IndexOf(collNavTileID, from), Array.IndexOf(collNavTileID, to)] > 0;
		}
	}
}
