using Godot;
using Game.Actor;
using Game.Database;
namespace Game.Ui
{
	public class DialogueController : GameMenu
	{
		private PopupController popupController;
		private MerchantController merchantController;
		private Control dialogueContent;
		private Npc npc;

		public override void _Ready()
		{
			popupController = GetNode<PopupController>("popup");
			popupController.Connect("hide", this, nameof(_OnDialogueHide));

			merchantController = GetNode<MerchantController>("merchant");
			merchantController.Connect("hide", this, nameof(_OnDialogueHide));

			dialogueContent = GetNode<Control>("s");
		}
		public void Init(InventoryModel playerInventory, InventoryModel playerSpellBook)
		{
			// merchant needs reference to player inventory/spells
			merchantController.playerInventory = playerInventory;
			merchantController.playerSpellBook = playerSpellBook;
		}
		public void Display(Npc npc)
		{
			// set to which npc we're speaking to
			this.npc = npc;
			merchantController.merchant = npc;
			dialogueContent.GetNode<Label>("control/header").Text = npc.worldName;

			// check if player could use some healing
			ContentDB.ContentNode contentNode = ContentDB.GetContentData(npc.Name);
			bool notFullHealth = player.hp < player.stats.hpMax.valueI;

			Label subHeader = dialogueContent.GetNode<Label>("control/sub_header");
			subHeader.Visible = contentNode.healer && notFullHealth;
			subHeader.Text = "Healer cost: " + contentNode.healerCost;
			dialogueContent.GetNode<Control>("s/v/heal").Visible = contentNode.healer && notFullHealth;

			// set dialogue
			dialogueContent.GetNode<RichTextLabel>("s/text").BbcodeText = contentNode.dialogue;

			// allow merchant view button if npc has merchandise to offer
			dialogueContent.GetNode<Control>("s/v/buy").Visible = contentNode.merchandise.Count > 0;
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
				GetNode<Label>("s/control/sub_header").Hide();
				GetNode<Control>("s/s/v/heal").Hide();
			}
		}
		public void _OnBuyPressed()
		{
			// called for when user wants to buy
			merchantController.DisplayItems(npc.worldName,
				ContentDB.GetContentData(npc.Name).merchandise.ToArray());

			// switch to merchant view
			dialogueContent.Hide();
			merchantController.Show();
		}
		public void _OnAcceptPressed() { /* TODO: quest code */ }
		public void _OnFinishPressed() { /* TODO: quest code */ }
	}
}