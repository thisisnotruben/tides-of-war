using Game.Database;
using Godot;
using System;
using System.Collections.Generic;
namespace Game.Quest
{
	public class QuestMaster
	{
		public enum QuestStatus : byte { UNAVAILABLE, AVAILABLE, ACTIVE, COMPLETED, DELIVERED }
		private readonly List<WorldQuest> quests = new List<WorldQuest>();

		public QuestMaster()
		{
			Directory directory = new Directory();
			directory.Open(PathManager.questDir);
			directory.ListDirBegin(true, true);

			string resourceName = directory.GetNext();

			// load all quest files
			Globals.questDB.data.Clear();
			while (!resourceName.Empty())
			{
				if (resourceName.Extension().Equals(PathManager.dataExt))
				{
					Globals.questDB.LoadData(PathManager.questDir.PlusFile(resourceName));
				}
				resourceName = directory.GetNext();
			}
			directory.ListDirEnd();

			// put all quest data to WorldQuests class and query for dependent quests
			Dictionary<string, QuestDB.QuestData> questData = Globals.questDB.data;
			HashSet<string> dependentQuests = new HashSet<string>();

			foreach (string questName in questData.Keys)
			{
				quests.Add(new WorldQuest(questData[questName]));

				if (!questData[questName].nextQuest.Empty())
				{
					dependentQuests.Add(questData[questName].nextQuest);
				}

				foreach (string startQuest in questData[questName].startQuests)
				{
					dependentQuests.Add(startQuest);
				}
			}

			// make quest availables that are independent from other quests
			quests.ForEach(q =>
			{
				if (!dependentQuests.Contains(q.quest.questName))
				{
					q.status = QuestStatus.AVAILABLE;
				}
			});
		}
		/*
		* UTIL FUNCTIONS START
		*/
		public bool IsPartOfObjective(string characterPath, out WorldQuest quest, QuestDB.QuestType questType)
		{
			WorldQuest foundQuest = null;
			quests.ForEach(q =>
			{
				switch (q.status)
				{
					case QuestStatus.ACTIVE:
					case QuestStatus.COMPLETED:
						if (q.IsPartOfObjective(characterPath, questType))
						{
							foundQuest = q;
							return;
						}
						break;
				}
			});
			quest = foundQuest;
			return foundQuest != null;
		}
		public bool HasQuestToOffer(string characterPath, out WorldQuest quest)
		{
			WorldQuest foundQuest = null;
			quests.ForEach(q =>
			{
				if (q.status == QuestStatus.AVAILABLE
				&& q.quest.questGiverPath.Equals(characterPath))
				{
					foundQuest = q;
					return;
				}
			});
			quest = foundQuest;
			return foundQuest != null;
		}
		public bool HasOutstandingQuest(string characterPath, out WorldQuest quest)
		{
			WorldQuest foundQuest = null;
			quests.ForEach(q =>
			{
				switch (q.status)
				{
					case QuestStatus.ACTIVE:
					case QuestStatus.COMPLETED:
						if (q.quest.questGiverPath.Equals(characterPath))
						{
							foundQuest = q;
							return;
						}
						break;
				}
			});
			quest = foundQuest;
			return foundQuest != null;
		}
		public bool TryGetLastDeliveredQuest(string characterPath, out WorldQuest quest)
		{
			WorldQuest foundQuest = null;
			quests.ForEach(q =>
			{
				if (q.status == QuestStatus.DELIVERED
				&& q.quest.questGiverPath.Equals(characterPath)
				&& q.quest.nextQuest.Empty())
				{
					foundQuest = q;
					return;
				}
			});
			quest = foundQuest;
			return foundQuest != null;
		}
		public bool TryGetExtraQuestContent(string characterWorldName, out QuestDB.ExtraContentData extraContentData)
		{
			QuestDB.ExtraContentData foundContent = null;
			quests.ForEach(q =>
			{
				switch (q.status)
				{
					case QuestStatus.ACTIVE:
					case QuestStatus.COMPLETED:
						if (q.IsPartOfObjective(characterWorldName, QuestDB.QuestType.TALK))
						{
							foundContent = q.quest.objectives[characterWorldName].extraContent;
							return;
						}
						break;
				}
			});
			extraContentData = foundContent;
			return foundContent != null;
		}
		/*
		* PROGRESS CHECK START
		*/
		public bool CheckQuests(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			bool partOfQuest = false;
			quests.ForEach(q =>
			{
				if (q.status == QuestStatus.ACTIVE
				&& q.UpdateQuest(objectiveName, questType, countTowardsObjective))
				{
					partOfQuest = true;
				}
			});
			return partOfQuest;
		}
		public bool CheckQuests(string characterPath, string objectiveName, QuestDB.QuestType questType)
		{
			bool partOfQuest = false;
			quests.ForEach(q =>
			{
				if (q.status == QuestStatus.ACTIVE
				&& q.UpdateQuest(characterPath, objectiveName, questType, true))
				{
					partOfQuest = true;
				}
			});
			return partOfQuest;
		}
		/*
		* STATUS CHANGES
		*/
		public void ActivateQuest(string questName) { SwitchStatus(questName, QuestStatus.ACTIVE); }
		public WorldQuest DeliverQuest(string questName)
		{
			WorldQuest worldQuest = SwitchStatus(questName, QuestStatus.DELIVERED),
				chainedQuest = null;

			if (worldQuest != null
			&& TryGetQuestByName(worldQuest.quest.nextQuest, out chainedQuest))
			{
				chainedQuest.status = QuestStatus.AVAILABLE;
			}

			return chainedQuest;
		}
		private WorldQuest SwitchStatus(string questName, QuestStatus status)
		{
			WorldQuest worldQuest;
			if (TryGetQuestByName(questName, out worldQuest))
			{
				worldQuest.status = status;
			}
			return worldQuest;
		}
		private bool TryGetQuestByName(string questName, out WorldQuest quest)
		{
			WorldQuest foundQuest = null;
			quests.ForEach(q =>
			{
				if (questName.Equals(q.quest.questName))
				{
					foundQuest = q;
					return;
				}
			});
			quest = foundQuest;
			return foundQuest != null;
		}
	}
}