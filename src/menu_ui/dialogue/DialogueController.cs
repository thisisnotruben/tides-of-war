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
		private MerchantController merchantController;
		private Label header, subHeader;
		private RichTextLabel dialogue;
		private Control mainContent, heal, buy;

		private Npc npc;
		private WorldQuest worldQuest;

		public override void _Ready()
		{
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnDialogueHide));

			merchantController = GetNode<MerchantController>("merchant");
			merchantController.Connect("hide", this, nameof(_OnDialogueHide));

			mainContent = GetNode<Control>("s");
			header = mainContent.GetNode<Label>("control/header");
			subHeader = mainContent.GetNode<Label>("control/sub_header");
			dialogue = mainContent.GetNode<RichTextLabel>("s/text");

			heal = mainContent.GetNode<Control>("s/v/heal");
			buy = mainContent.GetNode<Control>("s/v/buy");
		}
		public void Init(InventoryModel playerInventory, InventoryModel playerSpellBook)
		{
			// merchant needs reference to player inventory/spells
			merchantController.playerInventory = playerInventory;
			merchantController.playerSpellBook = playerSpellBook;
		}
		public void Display(Npc npc)
		{
			this.npc = npc;
			merchantController.merchant = npc;

			header.Text = npc.worldName;

			if (ContentDB.Instance.HasData(npc.Name))
			{
				ContentDB.ContentData contentData = ContentDB.Instance.GetData(npc.Name);

				subHeader.Text = "Healer cost: " + contentData.healerCost;
				dialogue.BbcodeText = contentData.dialogue;

				subHeader.Visible = player.hp < player.stats.hpMax.valueI;
				heal.Visible = subHeader.Visible;
				buy.Visible = contentData.merchandise.Length > 0;
			}
			else
			{
				subHeader.Hide();
				heal.Hide();
				buy.Hide();
			}

			if (QuestMaster.TryGetQuest(npc.GetPath(), out worldQuest, true) && worldQuest.IsCompleted())
			{
				dialogue.Text = worldQuest.quest.completed;
			}
			else if (QuestMaster.TryGetQuest(npc.GetPath(), out worldQuest))
			{
				switch (worldQuest.status)
				{
					case QuestMaster.QuestStatus.ACTIVE:
						dialogue.Text = worldQuest.quest.active;
						break;
					case QuestMaster.QuestStatus.AVAILABLE:
						dialogue.Text = worldQuest.quest.available;
						break;
					case QuestMaster.QuestStatus.COMPLETED:
						dialogue.Text = worldQuest.quest.delivered;
						break;
				}
			}

			QuestDB.ExtraContentData extraContentData;
			if (QuestMaster.TryGetExtraQuestContent(npc.worldName, out extraContentData))
			{
				if (!extraContentData.dialogue.Empty())
				{
					dialogue.BbcodeText = extraContentData.dialogue;
				}

				if (!QuestMaster.HasTalkedTo(npc.GetPath(), npc.worldName, extraContentData))
				{
					if (extraContentData.gold > 0)
					{
						player.gold += extraContentData.gold;
						player.SpawnCombatText($"+{extraContentData.gold}", CombatText.TextType.GOLD);
					}
				}
			}
			QuestMaster.CheckQuests(npc.GetPath(), npc.worldName, QuestDB.QuestType.TALK);
		}
		public void _OnDialogueDraw() { PlaySound(NameDB.UI.TURN_PAGE); }
		public void _OnDialogueHide()
		{
			// provides a reset of all views
			popupController.Hide();
			merchantController.Hide();
			mainContent.Show();
		}
		public void _OnHealPressed()
		{
			int healerCost = ContentDB.Instance.GetData(npc.Name).healerCost;
			if (healerCost > player.gold)
			{
				// too expensive for player
				mainContent.Hide();
				popupController.ShowError("Not Enough\nGold!");
			}
			else
			{
				PlaySound(NameDB.UI.SELL_BUY);

				// player gives gold
				player.gold -= healerCost;
				// set player health to full
				player.hp = player.stats.hpMax.valueI;

				// hide label/buttons since already healed
				subHeader.Hide();
				heal.Hide();
			}
		}
		public void _OnBuyPressed()
		{
			// called for when user wants to buy
			merchantController.DisplayItems(npc.worldName,
				ContentDB.Instance.GetData(npc.Name).merchandise);

			// switch to merchant view
			mainContent.Hide();
			merchantController.Show();
		}
		public void _OnAcceptPressed()
		{
			if (worldQuest == null)
			{
				return;
			}
			PlaySound(NameDB.UI.QUEST_ACCEPT);
			QuestMaster.ActivateQuest(worldQuest.quest.questName);
		}
		public void _OnFinishPressed()
		{
			if (!worldQuest?.IsCompleted() ?? true)
			{
				return;
			}
			PlaySound(NameDB.UI.QUEST_FINISH);
			QuestMaster.CompleteQuest(worldQuest.quest.questName);
			player.gold += worldQuest.quest.goldReward;
			player.SpawnCombatText($"+{worldQuest.quest.goldReward}", CombatText.TextType.GOLD);
		}
	}
}