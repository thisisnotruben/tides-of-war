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
            ContentDB.ContentNode contentNode = ContentDB.GetContentData(npc.Name);
            GetNode<Label>("s/control/header").Text = npc.worldName;
            GetNode<Label>("s/control/sub_header").Visible = contentNode.healer;
            GetNode<Label>("s/control/sub_header").Text = "Healer cost: " + contentNode.healerCost;
            GetNode<RichTextLabel>("s/s/text").BbcodeText = contentNode.dialogue;
            GetNode<Control>("s/s/v/heal").Visible = contentNode.healer;
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