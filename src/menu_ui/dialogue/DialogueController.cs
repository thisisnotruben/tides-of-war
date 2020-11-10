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
		private Control dialogueContent;

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
				subHeader.Visible = player.hp < player.stats.hpMax.valueI;

				dialogue.BbcodeText = contentData.dialogue;

				dialogueContent.GetNode<Control>("s/v/heal").Visible = subHeader.Visible;
				dialogueContent.GetNode<Control>("s/v/buy").Visible = contentData.merchandise.Length > 0;
			}

			if (QuestMaster.TryGetActiveQuest(npc.Name, true, out worldQuest) && worldQuest.IsCompleted())
			{
				// quest completed
				dialogue.Text = worldQuest.quest.completed;
			}
			else if (QuestMaster.TryGetActiveQuest(npc.Name, false, out worldQuest))
			{
				// quest active
				dialogue.Text = worldQuest.quest.active;
			}
			else if (QuestMaster.TryGetAvailableQuest(npc.Name, out worldQuest))
			{
				// quest available
				dialogue.Text = worldQuest.quest.start;
			}
			else if (QuestMaster.TryGetCompletedQuest(npc.Name, out worldQuest))
			{
				// quest turned-in
				dialogue.Text = worldQuest.quest.delivered;
			}

			QuestDB.ExtraContentData extraContentData;
			if (QuestMaster.TryGetExtraQuestContent(npc.worldName, out extraContentData))
			{
				if (!extraContentData.dialogue.Empty())
				{
					dialogue.BbcodeText = extraContentData.dialogue;
				}

				if (!QuestMaster.HasTalkedTo(npc.GetPath(), npc.worldName))
				{
					// TODO: reward
					if (extraContentData.gold > 0)
					{
						player.gold += extraContentData.gold;
						player.SpawnCombatText($"+{extraContentData.gold}", CombatText.TextType.GOLD);
					}
				}
			}
			QuestMaster.CheckQuests(npc.GetPath(), npc.worldName, QuestDB.QuestType.TALK);
		}
		public void _OnDialogueDraw() { Globals.PlaySound("turn_page", this, speaker); }
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
				Globals.PlaySound("sell_buy", this, speaker);

				// player gives gold
				player.gold -= healerCost;
				// set player health to full
				player.hp = player.stats.hpMax.valueI;

				// hide label/buttons since already healed
				subHeader.Hide();
				GetNode<Control>("s/s/v/heal").Hide();
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