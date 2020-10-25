using System.Collections.Generic;
using System;
using Godot;
namespace Game.Database
{
	public static class ContentDB
	{
		public class ContentData
		{
			public readonly int level, healerCost;
			public readonly bool enemy, healer;
			public readonly string dialogue;
			public readonly string[] drops, spells, merchandise;

			public ContentData(int level, int healerCost, bool enemy, bool healer,
			string dialogue, string[] drops, string[] spells, string[] merchandise)
			{
				this.level = level;
				this.healerCost = healerCost;
				this.enemy = enemy;
				this.healer = healer;
				this.dialogue = dialogue;
				this.drops = drops;
				this.spells = spells;
				this.merchandise = merchandise;
			}
		}
		private static Dictionary<string, ContentData> contentData = new Dictionary<string, ContentData>();

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

				contentData.Add(characterName, new ContentData(
					level: (int)((Single)contentDict[nameof(ContentData.level)]),
					healerCost: (int)((Single)contentDict[nameof(ContentData.healerCost)]),
					enemy: (bool)contentDict[nameof(ContentData.enemy)],
					healer: (bool)contentDict[nameof(ContentData.healer)],
					dialogue: (string)contentDict[nameof(ContentData.dialogue)],
					drops: GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentData.drops)]),
					spells: GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentData.spells)]),
					merchandise: GetWorldNames((Godot.Collections.Array)contentDict[nameof(ContentData.merchandise)])
				));
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
		public static ContentData GetContentData(string editorName) { return contentData[editorName]; }
		public static bool HasContent(string nameCheck) { return contentData.ContainsKey(nameCheck); }
	}
}
