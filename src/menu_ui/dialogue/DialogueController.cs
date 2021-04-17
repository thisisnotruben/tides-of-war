using Godot;
using Game.Actor;
using Game.Actor.Doodads;
using Game.Database;
using Game.Quest;
namespace Game.Ui
{
	public class DialogueController : GameMenu
	{
		private PopupController popupController;
		private Label header, subHeader;
		private RichTextLabel richTextLabel;
		private Control mainContent, heal;
		public Button closeButton;

		private WorldQuest worldQuest;
		private Npc _npc;
		public Npc npc
		{
			get { return _npc; }
			set
			{
				_npc = value;
				Display(value);
			}
		}

		public override void _Ready()
		{
			mainContent = GetChild<Control>(0);
			header = mainContent.GetNode<Label>("vBoxContainer/header");
			subHeader = mainContent.GetNode<Label>("vBoxContainer/subHeader");
			richTextLabel = mainContent.GetNode<RichTextLabel>("vBoxContainer/text");
			heal = mainContent.GetNode<Control>("hBoxContainer/heal");
			closeButton = mainContent.GetNode<Button>("hBoxContainer/close");

			popupController = GetChild<PopupController>(1);
			popupController.Connect("hide", this, nameof(OnHide));
		}
		private void Display(Npc npc)
		{
			if (npc == null)
			{
				return;
			}

			header.Text = npc.worldName;
			// TODO
			// richTextLabel.BbcodeText = Globals.unitDB.GetData(npc.Name).dialogue;

			if (Globals.contentDB.HasData(npc.Name))
			{
				subHeader.Text = "Healer cost: " + Globals.contentDB.GetData(npc.Name).healerCost;
				heal.Visible = subHeader.Visible = player.hp < player.stats.hpMax.valueI;
			}
			else
			{
				subHeader.Visible = heal.Visible = false;
			}

			if (QuestMaster.TryGetQuest(npc.GetPath(), out worldQuest, true) && worldQuest.IsCompleted())
			{
				richTextLabel.Text = worldQuest.quest.completed;
			}
			else if (QuestMaster.TryGetQuest(npc.GetPath(), out worldQuest))
			{
				switch (worldQuest.status)
				{
					case QuestMaster.QuestStatus.ACTIVE:
						richTextLabel.Text = worldQuest.quest.active;
						break;
					case QuestMaster.QuestStatus.AVAILABLE:
						richTextLabel.Text = worldQuest.quest.available;
						break;
					case QuestMaster.QuestStatus.COMPLETED:
						richTextLabel.Text = worldQuest.quest.delivered;
						break;
				}
			}

			QuestDB.ExtraContentData extraContentData;
			if (QuestMaster.TryGetExtraQuestContent(npc.worldName, out extraContentData))
			{
				if (!extraContentData.dialogue.Empty())
				{
					richTextLabel.BbcodeText = extraContentData.dialogue;
				}

				if (!QuestMaster.HasTalkedTo(npc.GetPath(), npc.worldName, extraContentData))
				{
					if (extraContentData.gold > 0)
					{
						player.gold += extraContentData.gold;
						player.SpawnCombatText(extraContentData.gold.ToString(), CombatText.TextType.GOLD);
					}
				}
			}
			QuestMaster.CheckQuests(npc.GetPath(), npc.worldName, QuestDB.QuestType.TALK);
		}
		private void OnHide()
		{
			popupController.Hide();
			mainContent.Show();
		}
		public void OnHealPressed()
		{
			int healerCost = Globals.contentDB.GetData(npc.Name).healerCost;
			if (healerCost > player.gold)
			{
				mainContent.Hide();
				popupController.ShowError("Not Enough\nGold!");
			}
			else
			{
				PlaySound(NameDB.UI.SELL_BUY);
				player.gold -= healerCost;
				player.hp = player.stats.hpMax.valueI;
				subHeader.Visible = heal.Visible = false;
			}
		}
		public void OnAcceptPressed()
		{
			if (worldQuest != null)
			{
				PlaySound(NameDB.UI.QUEST_ACCEPT);
				QuestMaster.ActivateQuest(worldQuest.quest.questName);
			}
		}
		public void OnFinishPressed()
		{
			if (worldQuest?.IsCompleted() ?? false)
			{
				PlaySound(NameDB.UI.QUEST_FINISH);
				QuestMaster.CompleteQuest(worldQuest.quest.questName);
				player.gold += worldQuest.quest.goldReward;
				player.SpawnCombatText(worldQuest.quest.goldReward.ToString(), CombatText.TextType.GOLD);
			}
		}
	}
}