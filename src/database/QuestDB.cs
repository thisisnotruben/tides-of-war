using System;
using System.Collections.Generic;
using Godot;
namespace Game.Database
{
	public static class QuestDB
	{
		public enum QuestType : byte { TALK, KILL, COLLECT, LEARN }

		public class QuestData
		{
			public readonly string questName, questCompleter,
				// dialogue
				start, active, completed, delivered;
			public readonly string[] nextQuest, reward;
			public readonly bool keepRewardItems;
			public readonly int goldReward;
			public readonly Dictionary<string, ObjectiveData> objectives;

			public QuestData(string questName, string questCompleter, string start, string active,
			string completed, string delivered, string[] nextQuest, string[] reward,
			bool keepRewardItems, int goldReward, Dictionary<string, ObjectiveData> objectives)
			{
				this.questName = questName;
				this.questCompleter = questCompleter;
				this.start = start;
				this.active = active;
				this.completed = completed;
				this.delivered = delivered;
				this.nextQuest = nextQuest;
				this.reward = reward;
				this.keepRewardItems = keepRewardItems;
				this.goldReward = goldReward;
				this.objectives = objectives;
			}
		}
		public class ObjectiveData
		{
			public readonly bool keepWorldItems;
			public readonly QuestType questType;
			public readonly int amount;
			public readonly ExtraContentData extraContent;

			public ObjectiveData(bool keepWorldItems, QuestType questType, int amount, ExtraContentData extraContent)
			{
				this.keepWorldItems = keepWorldItems;
				this.questType = questType;
				this.amount = amount;
				this.extraContent = extraContent;
			}
		}
		public class ExtraContentData
		{
			public readonly string dialogue, reward;
			public readonly int gold;

			public ExtraContentData(string dialogue, string reward, int gold)
			{
				this.dialogue = dialogue;
				this.reward = reward;
				this.gold = gold;
			}
		}

		public static Dictionary<string, QuestData> LoadQuestData(string path)
		{
			File file = new File();
			file.Open(path, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Dictionary<string, QuestData> questData = new Dictionary<string, QuestData>();

			Godot.Collections.Dictionary objectivesDict, extraContentDict,
				dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			Dictionary<string, ObjectiveData> objectives;
			foreach (string characterName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[characterName];

				objectives = new Dictionary<string, ObjectiveData>();
				objectivesDict = (Godot.Collections.Dictionary)dict[nameof(QuestData.objectives)];
				foreach (string objectiveName in objectivesDict)
				{
					extraContentDict = (Godot.Collections.Dictionary)objectivesDict[nameof(ObjectiveData.extraContent)];

					objectives.Add(objectiveName, new ObjectiveData(
						keepWorldItems: (bool)objectivesDict[nameof(ObjectiveData.keepWorldItems)],
						questType: (QuestType)Enum.Parse(typeof(QuestType), (string)objectivesDict[nameof(ObjectiveData.questType)]),
						amount: (int)objectivesDict[nameof(ObjectiveData.amount)],
						extraContent: new ExtraContentData(
							dialogue: (string)extraContentDict[nameof(ExtraContentData.dialogue)],
							reward: (string)extraContentDict[nameof(ExtraContentData.reward)],
							gold: (int)extraContentDict[nameof(ExtraContentData.gold)]
						)
					));
				}

				questData.Add(characterName, new QuestData(
					questName: (string)dict[nameof(QuestData.questName)],
					nextQuest: ContentDB.GetWorldNames((Godot.Collections.Array)dict[nameof(QuestData.nextQuest)]),
					reward: ContentDB.GetWorldNames((Godot.Collections.Array)dict[nameof(QuestData.reward)]),
					keepRewardItems: (bool)dict[nameof(QuestData.keepRewardItems)],
					goldReward: (int)dict[nameof(QuestData.goldReward)],
					questCompleter: (string)dict[nameof(QuestData.questCompleter)],
					start: (string)((Godot.Collections.Dictionary)dict[""])[nameof(QuestData.start)],
					active: (string)((Godot.Collections.Dictionary)dict[""])[nameof(QuestData.active)],
					completed: (string)((Godot.Collections.Dictionary)dict[""])[nameof(QuestData.completed)],
					delivered: (string)((Godot.Collections.Dictionary)dict[""])[nameof(QuestData.delivered)],
					objectives: objectives
				));
			}
			return questData;
		}
	}
}