using Game.Database;
using System;
using System.Collections.Generic;
namespace Game.Quest
{
	public static class QuestMaster
	{
		public enum QuestStatus : byte { AVAILABLE, ACTIVE, COMPLETED }
		private static readonly Dictionary<QuestStatus, Dictionary<string, WorldQuest>> quests =
			new Dictionary<QuestStatus, Dictionary<string, WorldQuest>>();

		static QuestMaster()
		{
			foreach (QuestStatus questStatus in Enum.GetValues(typeof(QuestStatus)))
			{
				quests[questStatus] = new Dictionary<string, WorldQuest>();
			}
		}
		public static void Init(string dbPath)
		{
			Dictionary<string, QuestDB.QuestData> questData = QuestDB.LoadQuestData(dbPath);
			foreach (string questName in questData.Keys)
			{
				quests[QuestStatus.AVAILABLE].Add(questName, new WorldQuest(questData[questName]));
			}
		}
		public static void CheckQuests(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			foreach (WorldQuest worldQuest in quests[QuestStatus.ACTIVE].Values)
			{
				worldQuest.UpdateQuest(objectiveName, questType, countTowardsObjective);
			}
		}
		public static void ActivateQuest(string questName)
		{
			Dictionary<string, WorldQuest> availableQuests = quests[QuestStatus.AVAILABLE];
			WorldQuest worldQuest = availableQuests.ContainsKey(questName)
				? availableQuests[questName]
				: null;

			if (worldQuest != null)
			{
				availableQuests.Remove(questName);
				quests[QuestStatus.ACTIVE].Add(questName, worldQuest);
			}
		}
		public static void FinishQuest(string questName)
		{
			Dictionary<string, WorldQuest> activeQuests = quests[QuestStatus.ACTIVE];
			WorldQuest worldQuest = activeQuests.ContainsKey(questName)
				? activeQuests[questName]
				: null;

			if (worldQuest?.IsCompleted() ?? false)
			{
				activeQuests.Remove(questName);
				quests[QuestStatus.COMPLETED].Add(questName, worldQuest);
			}
		}
	}
}
