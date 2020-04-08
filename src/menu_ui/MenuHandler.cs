using Game.Utils;
using Game.Actor;
namespace Game.Ui
{
    public class MenuHandler : GameMenu
    {
        private MainMenu mainMenu;
        private HudStatus hudStatus;
        private Hud hud;
        public override void _Ready()
        {
            GameMenu.speaker = GetNode<Speaker>("speaker");
            hudStatus = GetNode<HudStatus>("c/hud_status");
            hud = GetNode<Hud>("c/hud");
            hud.ConnectButtons(this, nameof(_OnHudPausePressed),
                nameof(_OnHudPausePressed), nameof(_OnHudPausePressed));
            mainMenu = GetNode<MainMenu>("c/game_menu");
            mainMenu.Connect("draw", this, nameof(_OnMainMenuDraw));
            mainMenu.Connect("hide", this, nameof(_OnMainMenuHide));
        }
        public void _OnMainMenuDraw()
        {
            hud.Hide();
            hudStatus.Hide();
        }
        public void _OnMainMenuHide()
        {
            hud.Show();
            hudStatus.Show();
        }
        public void _OnHudPausePressed()
        {
            Globals.PlaySound("click5", this, speaker);
            mainMenu.Show();
        }
        public void _OnNpcInteract(Npc npc)
        {
            string signal = nameof(Character.UpdateHudStatus);
            string method = nameof(HudStatus._OnUpdateStatus);
            if (npc.IsConnected(signal, hudStatus, method))
            {
                npc.Disconnect(signal, hudStatus, method);
                hudStatus.npcStatus.Hide();
                player.target = null;
            }
            else
            {
                foreach (Godot.Collections.Dictionary connectionPacket in hudStatus.GetIncomingConnections())
                {
                    Npc otherNpc = connectionPacket["source"] as Npc;
                    if (otherNpc != null)
                    {
                        otherNpc.Disconnect(signal, hudStatus, method);    
                    }
                }  
                npc.Connect(signal, hudStatus, method);
                hudStatus.UpdateName(false, npc.worldName);
                hudStatus._OnUpdateStatus(npc, true, npc.hp, npc.hpMax);
                hudStatus._OnUpdateStatus(npc, false, npc.mana, npc.manaMax);
                mainMenu._OnNpcInteract(npc);
            }
        }
    }
}