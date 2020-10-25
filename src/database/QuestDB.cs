using System.Collections.Generic;
using Godot;
namespace Game.Database
{
	public static class QuestDB
	{
		public class QuestData
		{
			public readonly string questName, questCompleter;
			public readonly string[] nextQuest, reward;
			public readonly bool keepRewardItems;
			public readonly int goldReward;
			public readonly GiverDialogueNode giverDialogue;
			public readonly Dictionary<string, ObjectiveNode> objectives;

			public QuestData(string questName, string questCompleter, string[] nextQuest,
			string[] reward, bool keepRewardItems, int goldReward,
			GiverDialogueNode giverDialogue, Dictionary<string, ObjectiveNode> objectives)
			{
				this.questName = questName;
				this.questCompleter = questCompleter;
				this.nextQuest = nextQuest;
				this.reward = reward;
				this.keepRewardItems = keepRewardItems;
				this.goldReward = goldReward;
				this.giverDialogue = giverDialogue;
				this.objectives = objectives;
			}
		}
		public class GiverDialogueNode
		{
			public string start, active, completed, delivered;

			public GiverDialogueNode(string start, string active, string completed, string delivered)
			{
				this.start = start;
				this.active = active;
				this.completed = completed;
				this.delivered = delivered;
			}
		}
		public class ObjectiveNode
		{
			public readonly bool keepWorldItems;
			public readonly string questType;
			public readonly int amount;
			public readonly ExtraContentNode extraContent;

			public ObjectiveNode(bool keepWorldItems, string questType, int amount, ExtraContentNode extraContent)
			{
				this.keepWorldItems = keepWorldItems;
				this.questType = questType;
				this.amount = amount;
				this.extraContent = extraContent;
			}
		}
		public class ExtraContentNode
		{
			public readonly string dialogue, reward;
			public readonly int gold;

			public ExtraContentNode(string dialogue, string reward, int gold)
			{
				this.dialogue = dialogue;
				this.reward = reward;
				this.gold = gold;
			}
		}
		public static Dictionary<string, QuestData> questData = new Dictionary<string, QuestData>();

		public static void LoadQuestData(string dbPath)
		{
			// clear out cached database for switching between maps
			questData.Clear();
			// load & parse data
			File file = new File();
			file.Open(dbPath, File.ModeFlags.Read);
			JSONParseResult jSONParseResult = JSON.Parse(file.GetAsText());
			file.Close();

			Godot.Collections.Dictionary objectivesDict, extraContentDict,
				dict, rawDict = (Godot.Collections.Dictionary)jSONParseResult.Result;
			Dictionary<string, ObjectiveNode> objectives;
			foreach (string characterName in rawDict.Keys)
			{
				dict = (Godot.Collections.Dictionary)rawDict[characterName];

				objectives = new Dictionary<string, ObjectiveNode>();
				objectivesDict = (Godot.Collections.Dictionary)dict[nameof(QuestData.objectives)];
				foreach (string objectiveName in objectivesDict)
				{
					extraContentDict = (Godot.Collections.Dictionary)objectivesDict[nameof(ObjectiveNode.extraContent)];

					objectives.Add(objectiveName, new ObjectiveNode(
						keepWorldItems: (bool)objectivesDict[nameof(ObjectiveNode.keepWorldItems)],
						questType: (string)objectivesDict[nameof(ObjectiveNode.questType)],
						amount: (int)objectivesDict[nameof(ObjectiveNode.amount)],
						extraContent: new ExtraContentNode(
							dialogue: (string)extraContentDict[nameof(ExtraContentNode.dialogue)],
							reward: (string)extraContentDict[nameof(ExtraContentNode.reward)],
							gold: (int)extraContentDict[nameof(ExtraContentNode.gold)]
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
					giverDialogue: new GiverDialogueNode(
						start: (string)((Godot.Collections.Dictionary)dict[nameof(QuestData.giverDialogue)])[nameof(GiverDialogueNode.start)],
						active: (string)((Godot.Collections.Dictionary)dict[nameof(QuestData.giverDialogue)])[nameof(GiverDialogueNode.active)],
						completed: (string)((Godot.Collections.Dictionary)dict[nameof(QuestData.giverDialogue)])[nameof(GiverDialogueNode.completed)],
						delivered: (string)((Godot.Collections.Dictionary)dict[nameof(QuestData.giverDialogue)])[nameof(GiverDialogueNode.delivered)]
					),
					objectives: objectives
				));
			}
		}
		public static QuestData GetQuestData(string editorName) { return questData[editorName]; }
		public static bool HasQuest(string nameCheck) { return questData.ContainsKey(nameCheck); }
	}
}