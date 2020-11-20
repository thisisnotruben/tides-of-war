using Game.Database;
using Godot;
using Game.Ui;
using System.Collections.Generic;
namespace Game.Quest
{
	public static class QuestMaster
	{
		public enum QuestStatus : byte { UNAVAILABLE, AVAILABLE, ACTIVE, COMPLETED }
		private static readonly List<WorldQuest> quests = new List<WorldQuest>();

		static QuestMaster()
		{
			Directory directory = new Directory();
			directory.Open(PathManager.questDir);
			directory.ListDirBegin(true, true);

			string resourceName = directory.GetNext();

			Dictionary<string, QuestDB.QuestData> questData;
			while (!resourceName.Empty())
			{
				if (resourceName.Extension().Equals(PathManager.dataExt))
				{
					QuestDB.Instance.LoadData(PathManager.questDir.PlusFile(resourceName));
					questData = QuestDB.Instance.data;
					foreach (string questName in questData.Keys)
					{
						quests.Add(new WorldQuest(questData[questName]));
					}
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();
		}
		private static bool TryGetQuestByName(string questName, out WorldQuest worldQuest)
		{
			foreach (WorldQuest q in quests)
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
			quests.ForEach(q =>
			{
				if (q.status == QuestStatus.ACTIVE)
				{
					q.UpdateQuest(objectiveName, questType, countTowardsObjective);
				}
			});
		}
		public static void CheckQuests(NodePath characterPath, string objectiveName, QuestDB.QuestType questType)
		{
			quests.ForEach(q =>
			{
				if (q.status == QuestStatus.ACTIVE)
				{
					q.UpdateQuest(characterPath, objectiveName, questType, true);
				}
			});
		}
		public static void ActivateQuest(string questName)
		{
			WorldQuest worldQuest;
			if (TryGetQuestByName(questName, out worldQuest))
			{
				worldQuest.status = QuestStatus.ACTIVE;
			}
		}
		public static void CompleteQuest(string questName)
		{
			WorldQuest worldQuest;
			if (TryGetQuestByName(questName, out worldQuest) && worldQuest.IsCompleted())
			{
				worldQuest.status = QuestStatus.COMPLETED;

				// move all chained quests to available status
				WorldQuest chainedQuest;
				foreach (string chainedQuestName in worldQuest.quest.nextQuest)
				{
					if (TryGetQuestByName(chainedQuestName, out chainedQuest))
					{
						chainedQuest.status = QuestStatus.AVAILABLE;
					}
				}
			}
		}
		public static bool TurnInItems(string questName, InventoryModel playerInventory)
		{
			WorldQuest worldQuest;
			TryGetQuestByName(questName, out worldQuest);
			worldQuest?.TurnInItems(playerInventory);
			return worldQuest != null;
		}
		public static bool TryGetExtraQuestContent(string characterWorldName, out QuestDB.ExtraContentData extraContentData)
		{
			foreach (WorldQuest worldQuest in quests)
			{
				if (worldQuest.status == QuestStatus.ACTIVE
				&& worldQuest.quest.objectives.ContainsKey(characterWorldName))
				{
					extraContentData = worldQuest.quest.objectives[characterWorldName].extraContent;
					return true;
				}
			}
			extraContentData = null;
			return false;
		}
		public static bool HasTalkedTo(NodePath characterPath, string characterWorldName, QuestDB.ExtraContentData extraContentData)
		{
			foreach (WorldQuest worldQuest in quests)
			{
				if (worldQuest.status == QuestStatus.ACTIVE
				&& worldQuest.quest.objectives.ContainsKey(characterWorldName)
				&& worldQuest.quest.objectives[characterWorldName].extraContent == extraContentData
				&& worldQuest.HasCharacterPath(characterPath))
				{
					return true;
				}
			}
			return false;
		}
		public static bool TryGetQuest(NodePath characterPath, out WorldQuest quest, bool questCompleter = false)
		{
			foreach (WorldQuest worldQuest in quests)
			{
				if ((questCompleter && worldQuest.quest.questCompleter.Equals(characterPath) && worldQuest.status == QuestStatus.ACTIVE)
				|| (!questCompleter && worldQuest.quest.questGiver.Equals(characterPath)))
				{
					quest = worldQuest;
					return true;
				}
			}
			quest = null;
			return false;
		}
		public static bool HasQuestOrQuestExtraContent(string characterWorldName, NodePath characterPath)
		{
			return TryGetQuest(characterPath, out _)
			|| TryGetQuest(characterPath, out _, true)
			|| TryGetExtraQuestContent(characterWorldName, out _);
		}
	}
}