using Game.Database;
using Godot;
using Game.Ui;
using System.Collections.Generic;
namespace Game.Quest
{
	public static class QuestMaster
	{
		public enum QuestStatus : byte { UNAVAILABLE, AVAILABLE, ACTIVE, COMPLETED }
		private static readonly List<WorldQuest> unavailableQuests = new List<WorldQuest>(),
			availableQuests = new List<WorldQuest>(),
			activeQuests = new List<WorldQuest>(),
			completedQuests = new List<WorldQuest>();

		public static void Init(string dbPath)
		{
			Dictionary<string, QuestDB.QuestData> questData = QuestDB.LoadQuestData(dbPath);
			foreach (string questName in questData.Keys)
			{
				unavailableQuests.Add(new WorldQuest(questData[questName]));
			}
		}
		private static bool TryGetQuest(string questName, List<WorldQuest> questHolder, out WorldQuest worldQuest)
		{
			foreach (WorldQuest q in questHolder)
			{
				if (questName.Equals(q.quest.questName))
				{
					worldQuest = q;
					return true;
				}
			}
			worldQuest = null;
			return false;
		}
		public static void CheckQuests(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			activeQuests.ForEach(q => q.UpdateQuest(objectiveName, questType, countTowardsObjective));
		}
		public static void CheckQuests(NodePath characterPath, string objectiveName, QuestDB.QuestType questType)
		{
			activeQuests.ForEach(q => q.UpdateQuest(characterPath, objectiveName, questType, true));
		}
		public static void ActivateQuest(string questName)
		{
			WorldQuest worldQuest;
			if (TryGetQuest(questName, availableQuests, out worldQuest))
			{
				availableQuests.Remove(worldQuest);
				activeQuests.Add(worldQuest);
			}
		}
		public static void CompleteQuest(string questName)
		{
			WorldQuest worldQuest;
			if (TryGetQuest(questName, activeQuests, out worldQuest) && worldQuest.IsCompleted())
			{
				activeQuests.Remove(worldQuest);
				completedQuests.Add(worldQuest);

				// move all chained quests to available status
				WorldQuest chainedQuest;
				foreach (string chainedQuestName in worldQuest.quest.nextQuest)
				{
					if (TryGetQuest(chainedQuestName, unavailableQuests, out chainedQuest))
					{
						unavailableQuests.Remove(chainedQuest);
						availableQuests.Add(chainedQuest);
					}
				}
			}
		}
		public static bool TurnInItems(string questName, InventoryModel playerInventory)
		{
			WorldQuest worldQuest;
			TryGetQuest(questName, completedQuests, out worldQuest);
			worldQuest?.TurnInItems(playerInventory);
			return worldQuest != null;
		}
		public static bool TryGetExtraQuestContent(string characterWorldName, out QuestDB.ExtraContentData extraContentData)
		{
			foreach (WorldQuest worldQuest in activeQuests)
			{
				if (worldQuest.quest.objectives.ContainsKey(characterWorldName))
				{
					extraContentData = worldQuest.quest.objectives[characterWorldName].extraContent;
					return true;
				}
			}
			extraContentData = null;
			return false;
		}
		public static bool HasTalkedTo(NodePath characterPath, string characterWorldName)
		{
			foreach (WorldQuest worldQuest in activeQuests)
			{
				if (worldQuest.quest.objectives.ContainsKey(characterWorldName) && worldQuest.HasCharacterPath(characterPath))
				{
					return true;
				}
			}
			return false;
		}
		public static bool TryGetAvailableQuest(string characterEditorName, out WorldQuest quest)
		{
			foreach (WorldQuest worldQuest in availableQuests)
			{
				if (worldQuest.quest.questGiver.Equals(characterEditorName))
				{
					quest = worldQuest;
					return true;
				}
			}
			quest = null;
			return false;
		}
		public static bool TryGetActiveQuest(string characterEditorName, bool questCompleter, out WorldQuest quest)
		{
			foreach (WorldQuest worldQuest in activeQuests)
			{
				if (questCompleter)
				{
					if (worldQuest.quest.questCompleter.Equals(characterEditorName))
					{
						quest = worldQuest;
						return true;
					}
				}
				else
				{
					if (worldQuest.quest.questGiver.Equals(characterEditorName))
					{
						quest = worldQuest;
						return true;
					}
				}
			}
			quest = null;
			return false;
		}
		public static bool TryGetCompletedQuest(string characterEditorName, out WorldQuest quest)
		{
			foreach (WorldQuest worldQuest in completedQuests)
			{
				if (worldQuest.quest.questGiver.Equals(characterEditorName))
				{
					quest = worldQuest;
					return true;
				}
			}
			quest = null;
			return false;
		}
		public static bool HasQuestOrQuestExtraContent(string characterWorldName, string characterEditorName)
		{
			return TryGetActiveQuest(characterEditorName, false, out _)
			|| TryGetActiveQuest(characterEditorName, true, out _)
			|| TryGetAvailableQuest(characterEditorName, out _)
			|| TryGetCompletedQuest(characterEditorName, out _)
			|| TryGetExtraQuestContent(characterWorldName, out _);
		}
	}
}