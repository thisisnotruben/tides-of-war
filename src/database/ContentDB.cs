using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class ContentDB
	{
		public struct ContentNode
		{
			public int level, healerCost;
			public bool enemy, healer;
			public string dialogue;
			public string[] drops, spells, merchandise;
		}
		private static Dictionary<string, ContentNode> contentData = new Dictionary<string, ContentNode>();

		public static void LoadContentData(string dbPath)
		{
			// clear out cached database for switching between maps
			contentData.Clear();
			// load & parse data
			File file = new File();
			file.Open(dbPath, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary contentDict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string characterName in rawDict.Keys)
			{
				contentDict = (Godot.Collections.Dictionary)rawDict[characterName];
				ContentNode contentNode;
				contentNode.level = (int)((Single)contentDict[nameof(ContentNode.level)]);
				contentNode.enemy = (bool)contentDict[nameof(ContentNode.enemy)];
				contentNode.healer = (bool)contentDict[nameof(ContentNode.healer)];
				contentNode.healerCost = (int)((Single)contentDict[nameof(ContentNode.healerCost)]);
				contentNode.dialogue = (string)contentDict[nameof(ContentNode.dialogue)];

				contentNode.drops = GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentNode.drops)]);
				contentNode.spells = GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentNode.spells)]);
				contentNode.merchandise = GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentNode.merchandise)]);

				contentData.Add(characterName, contentNode);
			}
		}
		public static string[] GetWorldNames(Godot.Collections.Array inArray)
		{
			string[] worldNames = new String[inArray.Count];
			for (int i = 0; i < worldNames.Length; i++)
			{
				worldNames[i] = (string)inArray[i];
			}
			return worldNames;
		}
		public static ContentNode GetContentData(string editorName) { return contentData[editorName]; }
		public static bool HasContent(string nameCheck) { return contentData.ContainsKey(nameCheck); }
	}
}
