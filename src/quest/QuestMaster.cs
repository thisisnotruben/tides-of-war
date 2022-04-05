using Game.Database;
using Game.Map.Doodads;
using Game.Actor;
using Game.Factory;
using Game.Ui;
using Game.Actor.Doodads;
using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
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
				q.Name = q.quest.dialogue;

				if (!dependentQuests.Contains(q.quest.questName))
				{
					q.status = QuestStatus.AVAILABLE;
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
		public void ShowQuestMarkers()
		{
			QuestMarker.MarkerType markerType;
			foreach (WorldQuest worldQuest in quests)
			{
				switch (worldQuest.status)
				{
					case QuestStatus.AVAILABLE:
						markerType = QuestMarker.MarkerType.AVAILABLE;
						break;
					case QuestStatus.ACTIVE:
						markerType = QuestMarker.MarkerType.ACTIVE;
						break;
					case QuestStatus.COMPLETED:
						markerType = QuestMarker.MarkerType.COMPLETED;
						break;
					default:
						continue;
				}
				GetNodeOrNull<Npc>(worldQuest.quest.questGiverPath)?.questMarker.ShowMarker(markerType);
			}
		}
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

			Node node;
			InteractItem interactItem;
			foreach (MapQuestItemDB.QuestItem item in Globals.mapQuestItemLootDB.GetData(worldQuest.quest.dialogue).mapItems)
			{
				node = Map.Map.map.GetGameChild(item.name);
				if (node != null)
				{
					interactItem = node as InteractItem;
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
		public void ActivateQuest(string questName)
		{
			AcitvateQuestMapItems(SwitchStatus(questName, QuestStatus.ACTIVE), true);
		}
		public WorldQuest DeliverQuest(string questName)
		{
			WorldQuest worldQuest = SwitchStatus(questName, QuestStatus.DELIVERED),
				chainedQuest = null;

			AcitvateQuestMapItems(worldQuest, false);

			if (worldQuest != null)
			{
				quests.ForEach(q =>
				{
					if (q != worldQuest)
					{
						q.UpdateQuest(NameDB.Item.QUEST_FINISH, QuestDB.QuestType.COLLECT, true);
					}
				});

				if (TryGetQuestByName(worldQuest.quest.nextQuest, out chainedQuest))
				{
					if (chainedQuest.status == QuestStatus.UNAVAILABLE)
					{
						chainedQuest.status = QuestStatus.AVAILABLE;
					}
					else
					{
						chainedQuest = null;
					}
				}
			}

			return chainedQuest;
		}
		private WorldQuest SwitchStatus(string questName, QuestStatus status)
		{
			WorldQuest worldQuest;
			if (TryGetQuestByName(questName, out worldQuest))
			{
				worldQuest.status = status;
				worldQuest.SetEvents();
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

		public GC.Dictionary Serialize()
		{
			GC.Array questsToSave = new GC.Array();
			quests.ForEach(q =>
			{
				if (q.status != QuestStatus.UNAVAILABLE)
				{
					questsToSave.Add(q.Serialize());
				}
			});
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