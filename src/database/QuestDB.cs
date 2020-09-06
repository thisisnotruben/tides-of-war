using System.Collections.Generic;
using Godot;
namespace Game.Database
{
	public static class QuestDB
	{
		public struct QuestNode
		{
			public string questName, questCompleter;
			public string[] nextQuest, reward;
			public bool keepRewardItems;
			public int goldReward;
			public GiverDialogueNode giverDialogue;
			public Dictionary<string, ObjectiveNode> objectives;
		}
		public struct GiverDialogueNode
		{
			public string start, active, completed, delivered;
		}
		public struct ObjectiveNode
		{
			public bool keepWorldItems;
			public string questType;
			public int amount;
			public ExtraContentNode extraContent;
		}
		public struct ExtraContentNode
		{
			public string dialogue, reward;
			public int gold;
		}
		public static Dictionary<string, QuestNode> questData = new Dictionary<string, QuestNode>();

		public static void LoadQuestData(string dbPath)
		{
			// clear out cached database for switching between maps
			questData.Clear();
			// load & parse data
			File file = new File();
			file.Open(dbPath, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			Godot.Collections.Dictionary questDict;
			foreach (string characterName in rawDict.Keys)
			{
				questDict = (Godot.Collections.Dictionary)rawDict[characterName];
				// load quest node
				QuestNode questNode;
				questNode.questName = (string)questDict[nameof(QuestNode.questName)];
				questNode.nextQuest = ContentDB.GetWorldNames((Godot.Collections.Array)questDict[nameof(QuestNode.nextQuest)]);
				questNode.reward = ContentDB.GetWorldNames((Godot.Collections.Array)questDict[nameof(QuestNode.reward)]);
				questNode.keepRewardItems = (bool)questDict[nameof(QuestNode.keepRewardItems)];
				questNode.goldReward = (int)questDict[nameof(QuestNode.goldReward)];
				questNode.questCompleter = (string)questDict[nameof(QuestNode.questCompleter)];
				// load dialogue
				GiverDialogueNode giverDialogueNode;
				giverDialogueNode.start = (string)((Godot.Collections.Dictionary)questDict[nameof(QuestNode.giverDialogue)])[nameof(GiverDialogueNode.start)];
				giverDialogueNode.active = (string)((Godot.Collections.Dictionary)questDict[nameof(QuestNode.giverDialogue)])[nameof(GiverDialogueNode.active)];
				giverDialogueNode.completed = (string)((Godot.Collections.Dictionary)questDict[nameof(QuestNode.giverDialogue)])[nameof(GiverDialogueNode.completed)];
				giverDialogueNode.delivered = (string)((Godot.Collections.Dictionary)questDict[nameof(QuestNode.giverDialogue)])[nameof(GiverDialogueNode.delivered)];
				questNode.giverDialogue = giverDialogueNode;
				// load all objectives
				questNode.objectives = new Dictionary<string, ObjectiveNode>();
				Godot.Collections.Dictionary objectivesDict = (Godot.Collections.Dictionary)questDict[nameof(QuestNode.objectives)];
				foreach (string objectiveName in objectivesDict)
				{
					// load objective
					ObjectiveNode objectiveNode;
					objectiveNode.keepWorldItems = (bool)objectivesDict[nameof(ObjectiveNode.keepWorldItems)];
					objectiveNode.questType = (string)objectivesDict[nameof(ObjectiveNode.questType)];
					objectiveNode.amount = (int)objectivesDict[nameof(ObjectiveNode.amount)];
					// load extra content
					Godot.Collections.Dictionary extraContentDict = (Godot.Collections.Dictionary)objectivesDict[nameof(ObjectiveNode.extraContent)];
					ExtraContentNode extraContent;
					extraContent.dialogue = (string)extraContentDict[nameof(ExtraContentNode.dialogue)];
					extraContent.reward = (string)extraContentDict[nameof(ExtraContentNode.reward)];
					extraContent.gold = (int)extraContentDict[nameof(ExtraContentNode.gold)];
					objectiveNode.extraContent = extraContent;
					// add to objectives
					questNode.objectives.Add(objectiveName, objectiveNode);
				}
				// add quest to cache
				questData.Add(characterName, questNode);
			}
		}
		public static QuestNode GetQuestData(string editorName) { return questData[editorName]; }
		public static bool HasQuest(string nameCheck) { return questData.ContainsKey(nameCheck); }
	}
}