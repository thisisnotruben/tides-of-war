using Godot;
using Game.Actor;
namespace Game.Ui
{
    public class HudStatus : GameMenu
    {
        private  Control playerStatus;
        public Control npcStatus;

        public override void _Ready()
        {
            playerStatus = GetNode<Control>("m/h/player_status");
            npcStatus = GetNode<Control>("m/h/npc_status");
            npcStatus.GetNode<BaseButton>("container/bg/slot_button")
                .Connect("pressed", this, nameof(_OnHideNPCStatusHudPressed));
        }
        public void _OnHideNPCStatusHudPressed()
        {
            Globals.PlaySound("click5", this, speaker);
            npcStatus.Hide();
            player.target = null;
        }
        public void UpdateName(bool player, string worldName)
        {
            string labelPath = "container/bg/m/v/name_header";
            Label label = (player)
                ? playerStatus.GetNode<Label>(labelPath)
                : npcStatus.GetNode<Label>(labelPath);
            label.Text = worldName;
            if (!player)
            {
                npcStatus.Show();
            }
        }
        public void _OnUpdateStatus(Character character, bool hp, int currentValue, int maxValue)
        {
            string labelPath = (hp)
                ? "container/bg/m/v/health_bar/health_header"
                : "container/bg/m/v/mana_bar/mana_header";
            string barPath = (hp)
                ? "container/bg/m/v/health_bar"
                : "container/bg/m/v/mana_bar";
            Label label;
            Range progressBar;
            if (character == player)
            {
                label = playerStatus.GetNode<Label>(labelPath);
                progressBar = playerStatus.GetNode<Range>(barPath);
            }
            else
            {
                label = npcStatus.GetNode<Label>(labelPath);
                progressBar = npcStatus.GetNode<Range>(barPath);
            }
            progressBar.Value = 100.0f * (float)currentValue / (float)maxValue;
            label.Text = $"{currentValue} / {maxValue}";
        }
    }
}
