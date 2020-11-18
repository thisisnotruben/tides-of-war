using Godot;
using Game.Actor;
using Game.Actor.Doodads;
using Game.Database;
using Game.Quest;
using Game.Sound;
namespace Game.Ui
{
	public class DialogueController : GameMenu
	{
		private PopupController popupController;
		private MerchantController merchantController;
		private Label header, subHeader;
		private RichTextLabel dialogue;
		private Control dialogueContent, heal, buy;

		private Npc npc;
		private WorldQuest worldQuest;

		public override void _Ready()
		{
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnDialogueHide));

			merchantController = GetNode<MerchantController>("merchant");
			merchantController.Connect("hide", this, nameof(_OnDialogueHide));

			dialogueContent = GetNode<Control>("s");
			header = dialogueContent.GetNode<Label>("control/header");
			subHeader = dialogueContent.GetNode<Label>("control/sub_header");
			dialogue = dialogueContent.GetNode<RichTextLabel>("s/text");

			heal = dialogueContent.GetNode<Control>("s/v/heal");
			buy = dialogueContent.GetNode<Control>("s/v/buy");
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

			if (ContentDB.HasContent(npc.Name))
			{
				ContentDB.ContentData contentData = ContentDB.GetContentData(npc.Name);

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
		public void _OnDialogueDraw() { SoundPlayer.INSTANCE.PlaySound("turn_page"); }
		public void _OnDialogueHide()
		{
			// provides a reset of all views
			popupController.Hide();
			merchantController.Hide();
			dialogueContent.Show();
		}
		public void _OnHealPressed()
		{
			int healerCost = ContentDB.GetContentData(npc.Name).healerCost;
			if (healerCost > player.gold)
			{
				// too expensive for player
				dialogueContent.Hide();
				popupController.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
				popupController.GetNode<Control>("m/error").Show();
				popupController.Show();
			}
			else
			{
				SoundPlayer.INSTANCE.PlaySound("sell_buy");

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
				ContentDB.GetContentData(npc.Name).merchandise);

			// switch to merchant view
			dialogueContent.Hide();
			merchantController.Show();
		}
		public void _OnAcceptPressed()
		{
			if (worldQuest == null)
			{
				return;
			}
			QuestMaster.ActivateQuest(worldQuest.quest.questName);
		}
		public void _OnFinishPressed()
		{
			if (!worldQuest?.IsCompleted() ?? true)
			{
				return;
			}
			QuestMaster.CompleteQuest(worldQuest.quest.questName);
			player.gold += worldQuest.quest.goldReward;
			player.SpawnCombatText($"+{worldQuest.quest.goldReward}", CombatText.TextType.GOLD);
		}
	}
}