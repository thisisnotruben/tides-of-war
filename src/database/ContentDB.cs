using System;
using GC = Godot.Collections;
namespace Game.Database
{
	public class ContentDB : AbstractDB<ContentDB.ContentData>
	{
		public class ContentData
		{
			public readonly bool healer;
			public readonly int healerCost;
			public readonly string[] drops, spells, merchandise;

			public ContentData(int healerCost, bool healer,
			string[] drops, string[] spells, string[] merchandise)
			{
				this.healerCost = healerCost;
				this.healer = healer;
				this.drops = drops;
				this.spells = spells;
				this.merchandise = merchandise;
			}
		}

		public override void LoadData(string path)
		{
			data.Clear();

			GC.Dictionary contentDict, rawDict = LoadJson(path);
			foreach (string characterName in rawDict.Keys)
			{
				contentDict = (GC.Dictionary)rawDict[characterName];

				data.Add(characterName, new ContentData(
					healerCost: (int)(Single)contentDict[nameof(ContentData.healerCost)],
					healer: (bool)contentDict[nameof(ContentData.healer)],
					drops: GetWorldNames((GC.Array)contentDict[nameof(ContentData.drops)]),
					spells: GetWorldNames((GC.Array)contentDict[nameof(ContentData.spells)]),
					merchandise: GetWorldNames((GC.Array)contentDict[nameof(ContentData.merchandise)])
				));
			}
		}
		public static string[] GetWorldNames(GC.Array inArray)
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
