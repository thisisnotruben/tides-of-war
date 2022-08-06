using Game.Database;
using Game.Map.Doodads;
using Game.Factory;
using Game.Ui;
using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Game.Quest
{
	public class QuestMaster : Node, ISerializable
	{
		public enum QuestStatus : byte { UNAVAILABLE, AVAILABLE, ACTIVE, COMPLETED, DELIVERED }
		private readonly List<WorldQuest> quests = new List<WorldQuest>();

		public override void _Ready()
		{
			Name = nameof(QuestMaster);
			AddToGroup(Globals.SAVE_GROUP);
			Globals.sceneLoader.Connect(nameof(SceneLoader.OnSetNewScene), this, nameof(RefreshQuestEvents));
		}
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
			ResetQuests();
		}
		private void ResetQuests()
		{
			quests.Clear();
			for (int i = 0; i < GetChildCount(); i++)
			{
				GetChild(i).QueueFree();
			}

			// put all quest data to WorldQuests class and query for dependent quests
			Dictionary<string, QuestDB.QuestData> questData = Globals.questDB.data;
			HashSet<string> dependentQuests = new HashSet<string>();
			QuestFactory questFactory = new QuestFactory();

			foreach (string questName in questData.Keys)
			{
				quests.Add(questFactory.Create(questData[questName].dialogue).Init(questData[questName]));

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
				AddChild(q);
				q.Name = q.quest.dialogue + "-" + q.quest.questName;

				if (!dependentQuests.Contains(q.quest.questName))
				{
					SwitchStatus(q, QuestStatus.AVAILABLE);
				}
			});
		}
		public void RefreshQuestEvents() { quests.ForEach(q => q.SetEvents()); }
		/*
		* UTIL FUNCTIONS START
		*/
		public List<WorldQuest> GetAllPlayerQuests()
		{
			List<WorldQuest> worldQuests = new List<WorldQuest>();
			quests.ForEach(q =>
			{
				switch (q.status)
				{
					case QuestStatus.ACTIVE:
					case QuestStatus.COMPLETED:
					case QuestStatus.DELIVERED:
						worldQuests.Add(q);
						break;
				}
			});
			return worldQuests;
		}
		public void ShowQuestMarkers() { quests.ForEach(q => q.ShowMarker()); }
		public bool IsPartOfObjective(string characterName, out WorldQuest quest, QuestDB.QuestType questType)
		{
			WorldQuest foundQuest = null;
			quests.ForEach(q =>
			{
				switch (q.status)
				{
					case QuestStatus.ACTIVE:
					case QuestStatus.COMPLETED:
						if (q.IsPartOfObjective(characterName, questType))
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
		private void AcitvateQuestMapItems(WorldQuest worldQuest, bool activate)
		{
			if (worldQuest == null
			|| !Globals.mapQuestItemLootDB.HasData(worldQuest.quest.dialogue)) // dialogue == quest id
			{
				return;
			}

			InteractItem interactItem;
			foreach (MapQuestItemDB.QuestItem item in Globals.mapQuestItemLootDB.GetData(worldQuest.quest.dialogue).mapItems)
			{
				interactItem = Map.Map.map.GetGameChild(item.name) as InteractItem;
				if (interactItem != null)
				{
					interactItem.Visible = activate;
					interactItem.SetInteractType(
						activate ? item.type : string.Empty,
						activate ? item.value : string.Empty
					);
				}
			}
		}
		/*
		* PROGRESS CHECK START
		*/
		public WorldQuest CheckQuests(string objectiveName, QuestDB.QuestType questType, bool countTowardsObjective)
		{
			foreach (WorldQuest worldQuest in quests)
			{
				if ((worldQuest.status == QuestStatus.ACTIVE || worldQuest.status == QuestStatus.COMPLETED)
				&& worldQuest.UpdateQuest(objectiveName, questType, countTowardsObjective))
				{
					return worldQuest;
				}
			}
			return null;
		}
		public bool CheckQuests(string characterPath, QuestDB.QuestType questType)
		{
			bool partOfQuest = false;
			NodePath characterNodePath = new NodePath(characterPath);
			string objectiveName = characterNodePath.GetName(characterNodePath.GetNameCount() - 1);
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
		public void AllowStartQuests(WorldQuest worldQuest)
		{
			worldQuest.quest.startQuests.ToList().ForEach(questName => SwitchStatus(questName, QuestStatus.AVAILABLE));
		}
		public void ActivateQuest(string questName) { SwitchStatus(questName, QuestStatus.ACTIVE); }
		public WorldQuest DeliverQuest(string questName)
		{
			WorldQuest worldQuest = SwitchStatus(questName, QuestStatus.DELIVERED),
				chainedQuest = null;

			if (worldQuest == null)
			{
				return null;
			}

			AcitvateQuestMapItems(worldQuest, false);

			quests.Where(q => q != worldQuest).ToList().ForEach(q =>
			{
				q.UpdateQuest(NameDB.Item.QUESTS_FINISHED, QuestDB.QuestType.TALK, true);

				if (worldQuest.quest.startQuests.Contains(q.quest.questName)
				&& q.status == QuestStatus.UNAVAILABLE)
				{
					SwitchStatus(q.quest.questName, QuestStatus.AVAILABLE);
				}
			});

			if (TryGetQuestByName(worldQuest.quest.nextQuest, out chainedQuest)
			&& chainedQuest.status == QuestStatus.UNAVAILABLE)
			{
				SwitchStatus(chainedQuest, QuestStatus.AVAILABLE);
			}

			return chainedQuest;
		}
		private WorldQuest SwitchStatus(string questName, QuestStatus status)
		{
			WorldQuest worldQuest;
			if (TryGetQuestByName(questName, out worldQuest))
			{
				SwitchStatus(worldQuest, status);
			}
			return worldQuest;
		}
		private void SwitchStatus(WorldQuest worldQuest, QuestStatus status)
		{
			worldQuest.status = status;
			worldQuest.SetEvents();

			switch (status)
			{
				case QuestStatus.ACTIVE:
					AcitvateQuestMapItems(worldQuest, true);
					break;
			}
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
		public GC.Dictionary Serialize()
		{
			GC.Array questsToSave = new GC.Array();
			quests.Where(q => q.status != QuestStatus.UNAVAILABLE).ToList()
				.ForEach(q => questsToSave.Add(q.Serialize()));
			return new GC.Dictionary() { { NameDB.SaveTag.QUESTS, questsToSave } };
		}
		public void Deserialize(GC.Dictionary payload)
		{
			ResetQuests();
			WorldQuest worldQuest;
			foreach (GC.Dictionary questData in payload)
			{
				if (TryGetQuestByName(questData[NameDB.SaveTag.NAME].ToString(), out worldQuest))
				{
					worldQuest.Deserialize(questData);
				}
			}
		}
	}
}