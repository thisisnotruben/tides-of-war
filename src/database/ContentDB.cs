using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class ContentDB
	{
		public struct ContentNode
		{
			public int level;
			public bool enemy;
			public bool healer;
			public int healerCost;
			public string dialogue;
			public List<string> drops;
			public List<string> spells;
			public List<string> merchandise;

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
			Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			foreach (string characterName in rawDict.Keys)
			{
				Godot.Collections.Dictionary contentDict = (Godot.Collections.Dictionary)rawDict[characterName];
				ContentNode contentNode;
				contentNode.level = (int)((Single)contentDict[nameof(ContentNode.level)]);
				contentNode.enemy = (bool)contentDict[nameof(ContentNode.enemy)];
				contentNode.healer = (bool)contentDict[nameof(ContentNode.healer)];
				contentNode.healerCost = (int)((Single)contentDict[nameof(ContentNode.healerCost)]);
				contentNode.dialogue = (string)contentDict[nameof(ContentNode.dialogue)];
				contentNode.drops = new List<string>();
				contentNode.spells = new List<string>();
				contentNode.merchandise = new List<string>();
				GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentNode.drops)], contentNode.drops);
				GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentNode.spells)], contentNode.spells);
				GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentNode.merchandise)], contentNode.merchandise);
				contentData.Add(characterName, contentNode);
			}
		}
		public static void GetWorldNames(Godot.Collections.Array inArray, List<string> outArray)
		{
			foreach (string worldObjectName in inArray)
			{
				outArray.Add(worldObjectName);
			}
		}
		public static ContentNode GetContentData(string editorName) { return contentData[editorName]; }
		public static bool HasContent(string nameCheck) { return contentData.ContainsKey(nameCheck); }
	}
}
