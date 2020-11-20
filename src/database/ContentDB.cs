using System;
namespace Game.Database
{
	public class ContentDB : AbstractDB<ContentDB.ContentData>
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

		public static readonly ContentDB Instance = new ContentDB();

		public override void LoadData(string path)
		{
			data.Clear();

			Godot.Collections.Dictionary contentDict, rawDict = LoadJson(path);
			foreach (string characterName in rawDict.Keys)
			{
				contentDict = (Godot.Collections.Dictionary)rawDict[characterName];

				data.Add(characterName, new ContentData(
					level: (int)(Single)contentDict[nameof(ContentData.level)],
					healerCost: (int)(Single)contentDict[nameof(ContentData.healerCost)],
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
	}
}
