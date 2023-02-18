using System;
using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;
namespace Game.Database
{
	public class QuestDB : AbstractDB<QuestDB.QuestData>
	{
		public enum QuestType : byte { TALK, KILL, COLLECT, LEARN, SCOUT }

		public class QuestData
		{
			public readonly string questGiverPath, questName, dialogue, nextQuest;
			public readonly string[] reward, startQuests;
			public readonly bool keepRewardItems;
			public readonly int goldReward;
			public readonly Dictionary<string, ObjectiveData> objectives;

			public QuestData(string questGiverPath, string questName,
			string dialogue, string nextQuest, string[] reward, string[] startQuests, bool keepRewardItems, int goldReward,
			Dictionary<string, ObjectiveData> objectives)
			{
				this.questGiverPath = questGiverPath;
				this.questName = questName;
				this.dialogue = dialogue;
				this.nextQuest = nextQuest;
				this.reward = reward;
				this.startQuests = startQuests;
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
			public readonly string reward;
			public readonly int gold;

			public ExtraContentData(string reward, int gold)
			{
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
							reward: extraContentDict[nameof(ExtraContentData.reward)].ToString(),
							gold: extraContentDict[nameof(ExtraContentData.gold)].ToString().ToInt()
						)
					));
				}

				data.Add(questName, new QuestData(
					questName: questName,
					nextQuest: dict[nameof(QuestData.nextQuest)].ToString(),
					reward: ContentDB.GetWorldNames((GC.Array)dict[nameof(QuestData.reward)]),
					startQuests: ContentDB.GetWorldNames((GC.Array)dict[nameof(QuestData.startQuests)]),
					keepRewardItems: (bool)dict[nameof(QuestData.keepRewardItems)],
					goldReward: dict[nameof(QuestData.goldReward)].ToString().ToInt(),
					questGiverPath: dict["questGiver"].ToString(),
					dialogue: dict[nameof(QuestData.dialogue)].ToString(),
					objectives: objectives
				));
			}
		}
	}
}