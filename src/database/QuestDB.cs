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
			public readonly NodePath questGiver, questCompleter;
			public readonly string questName, available, active, completed, delivered;
			public readonly string[] nextQuest, reward;
			public readonly bool keepRewardItems;
			public readonly int goldReward;
			public readonly Dictionary<string, ObjectiveData> objectives;

			public QuestData(NodePath questGiver, NodePath questCompleter, string questName,
			string available, string active, string completed, string delivered,
			string[] nextQuest, string[] reward, bool keepRewardItems, int goldReward,
			Dictionary<string, ObjectiveData> objectives)
			{
				this.questGiver = questGiver;
				this.questCompleter = questCompleter;
				this.questName = questName;
				this.available = available;
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
			public readonly bool keepWorldObject;
			public readonly QuestType questType;
			public readonly int amount;
			public readonly ExtraContentData extraContent;

			public ObjectiveData(bool keepWorldObject, QuestType questType, int amount, ExtraContentData extraContent)
			{
				this.keepWorldObject = keepWorldObject;
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

			Godot.Collections.Dictionary rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result,
				dict, objectivesDict, objective, extraContentDict;
			Dictionary<string, ObjectiveData> objectives;
			foreach (string questName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[questName];
				objectives = new Dictionary<string, ObjectiveData>();
				objectivesDict = (Godot.Collections.Dictionary)dict[nameof(QuestData.objectives)];

				foreach (string objectiveName in objectivesDict.Keys)
				{
					objective = (Godot.Collections.Dictionary)objectivesDict[objectiveName];
					extraContentDict = (Godot.Collections.Dictionary)objective[nameof(ObjectiveData.extraContent)];

					objectives.Add(objectiveName, new ObjectiveData(
						keepWorldObject: (bool)objective[nameof(ObjectiveData.keepWorldObject)],
						questType: (QuestType)Enum.Parse(typeof(QuestType), (string)objective[nameof(ObjectiveData.questType)]),
						amount: (int)(Single)objective[nameof(ObjectiveData.amount)],
						extraContent: new ExtraContentData(
							dialogue: (string)extraContentDict[nameof(ExtraContentData.dialogue)],
							reward: (string)extraContentDict[nameof(ExtraContentData.reward)],
							gold: (int)(Single)extraContentDict[nameof(ExtraContentData.gold)]
						)
					));
				}

				questData.Add(questName, new QuestData(
					questName: questName,
					nextQuest: ContentDB.GetWorldNames((Godot.Collections.Array)dict[nameof(QuestData.nextQuest)]),
					reward: ContentDB.GetWorldNames((Godot.Collections.Array)dict[nameof(QuestData.reward)]),
					keepRewardItems: (bool)dict[nameof(QuestData.keepRewardItems)],
					goldReward: (int)(Single)dict[nameof(QuestData.goldReward)],
					questGiver: new NodePath((string)dict[nameof(QuestData.questGiver)]),
					questCompleter: new NodePath((string)dict[nameof(QuestData.questCompleter)]),
					available: (string)dict[nameof(QuestData.available)],
					active: (string)dict[nameof(QuestData.active)],
					completed: (string)dict[nameof(QuestData.completed)],
					delivered: (string)dict[nameof(QuestData.delivered)],
					objectives: objectives
				));
			}
			return questData;
		}
	}
}