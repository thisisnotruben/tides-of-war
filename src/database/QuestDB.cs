using System;
using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;
namespace Game.Database
{
	public class QuestDB : AbstractDB<QuestDB.QuestData>
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

		public override void LoadData(string path)
		{
			GC.Dictionary rawDict = LoadJson(path),
				dict, objectivesDict, objective, extraContentDict;

			Dictionary<string, ObjectiveData> objectives;

			foreach (string questName in rawDict.Keys)
			{
				dict = (GC.Dictionary)rawDict[questName];
				objectives = new Dictionary<string, ObjectiveData>();
				objectivesDict = (GC.Dictionary)dict[nameof(QuestData.objectives)];

				foreach (string objectiveName in objectivesDict.Keys)
				{
					objective = (GC.Dictionary)objectivesDict[objectiveName];
					extraContentDict = (GC.Dictionary)objective[nameof(ObjectiveData.extraContent)];

					objectives.Add(objectiveName, new ObjectiveData(
						keepWorldObject: (bool)objective[nameof(ObjectiveData.keepWorldObject)],
						questType: (QuestType)Enum.Parse(typeof(QuestType), objective[nameof(ObjectiveData.questType)].ToString()),
						amount: objective[nameof(ObjectiveData.amount)].ToString().ToInt(),
						extraContent: new ExtraContentData(
							dialogue: extraContentDict[nameof(ExtraContentData.dialogue)].ToString(),
							reward: extraContentDict[nameof(ExtraContentData.reward)].ToString(),
							gold: extraContentDict[nameof(ExtraContentData.gold)].ToString().ToInt()
						)
					));
				}

				data.Add(questName, new QuestData(
					questName: questName,
					nextQuest: ContentDB.GetWorldNames((GC.Array)dict[nameof(QuestData.nextQuest)]),
					reward: ContentDB.GetWorldNames((GC.Array)dict[nameof(QuestData.reward)]),
					keepRewardItems: (bool)dict[nameof(QuestData.keepRewardItems)],
					goldReward: dict[nameof(QuestData.goldReward)].ToString().ToInt(),
					questGiver: new NodePath(dict[nameof(QuestData.questGiver)].ToString()),
					questCompleter: new NodePath(dict[nameof(QuestData.questCompleter)].ToString()),
					available: dict[nameof(QuestData.available)].ToString(),
					active: dict[nameof(QuestData.active)].ToString(),
					completed: dict[nameof(QuestData.completed)].ToString(),
					delivered: dict[nameof(QuestData.delivered)].ToString(),
					objectives: objectives
				));
			}
		}
	}
}