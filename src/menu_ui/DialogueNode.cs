using Godot;
using Game.Actor;
using Game.Database;
namespace Game.Ui
{
    public class DialogueNode : GameMenu
    {
        private Popup popup;
        private MerchantNode merchantNode;
        private Npc npc;

        public override void _Ready()
        {
            popup = GetNode<Popup>("popup");
            popup.Connect("hide", this, nameof(_OnDialogueHide));
            merchantNode = GetNode<MerchantNode>("merchant");
            merchantNode.Connect("hide", this, nameof(_OnDialogueHide));
        }
        public void Display(Npc npc)
        {
            this.npc = npc;
            merchantNode.merchant = npc;
            bool notFullHealth = player.hp < player.hpMax;
            ContentDB.ContentNode contentNode = ContentDB.GetContentData(npc.Name);
            GetNode<Label>("s/control/header").Text = npc.worldName;
            Label subHeader = GetNode<Label>("s/control/sub_header");
            subHeader.Visible = contentNode.healer && notFullHealth;
            subHeader.Text = "Healer cost: " + contentNode.healerCost;
            GetNode<RichTextLabel>("s/s/text").BbcodeText = contentNode.dialogue;
            GetNode<Control>("s/s/v/heal").Visible = contentNode.healer && notFullHealth;
            GetNode<Control>("s/s/v/buy").Visible = contentNode.merchandise.Count > 0;
        }
        public void InitMerchantView(ItemList inventoryItemList, ItemList spellBookItemList)
        {
            merchantNode.inventoryItemList = inventoryItemList;
            merchantNode.spellBookItemList = spellBookItemList;
        }
        public void _OnDialogueDraw()
        {
            Globals.PlaySound("turn_page", this, speaker);
        }
        public void _OnDialogueHide()
        {
            popup.Hide();
            merchantNode.Hide();
            GetNode<Control>("s").Show();
        }
        public void _OnHealPressed()
        {
            int healerCost = ContentDB.GetContentData(npc.Name).healerCost;
            if (healerCost > player.gold)
            {   
                GetNode<Control>("s").Hide();
                popup.GetNode<Label>("m/error/label").Text = "Not Enough\nGold!";
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
            else
            {
                Globals.PlaySound("sell_buy", this, speaker);
                player.gold -= healerCost;
                player.hp = player.hpMax;
                bool notFullHealth = player.hp < player.hpMax;
                GetNode<Label>("s/control/sub_header").Visible = notFullHealth;
                GetNode<Control>("s/s/v/heal").Visible = notFullHealth;
            }
        }
        public void _OnBuyPressed()
        {
            merchantNode.DisplayItems(npc.worldName,
                ContentDB.GetContentData(npc.Name).merchandise.ToArray());
            GetNode<Control>("s").Hide();
            merchantNode.Show();
        }
        public void _OnAcceptPressed()
        {
            // TODO: quest code
            // Globals.PlaySound("quest_accept", this, speaker);
            // Globals.worldQuests.StartFocusedQuest();
            // QuestEntry questEntry = (QuestEntry)questEntryScene.Instance();
            // questEntry.AddToQuestLog(questLog);
            // questEntry.quest = Globals.worldQuests.GetFocusedQuest();
            // GetNode<Control>("s/s/v/accept").Hide();
            // GetNode<Control>("s/s/v/finish").Hide();
            // Hide();
        }
        public void _OnFinishPressed()
        {
            // TODO: quest code
            // Quest quest = Globals.worldQuests.GetFocusedQuest();
            // if (quest.GetGold() > 0)
            // {
            //     Globals.PlaySound("sell_buy", this, snd);
            //     player.gold = quest.GetGold();
            //     CombatText combatText = (CombatText)Globals.combatText.Instance();
            //     player.AddChild(combatText);
            //     combatText.SetType($"+{quest.GetGold().ToString("N0")}",
            //         CombatText.TextType.GOLD, player.GetNode<Node2D>("img").Position);
            // }
            // Pickable questReward = quest.reward;
            // if (questReward != null)
            // {
            //     Globals.map.AddZChild(questReward);
            //     questReward.GlobalPosition = Globals.map.GetGridPosition(player.GlobalPosition);
            // }
            // Globals.worldQuests.FinishFocusedQuest();
            // if (Globals.worldQuests.GetFocusedQuest() != null)
            // {
            //     dialogue.GetNode<Label>("s/s/label2").Text = Globals.worldQuests.GetFocusedQuest().questStart;
            // }
            // else
            // {
            //     _OnBackPressed();
            // }
        }
    }
}