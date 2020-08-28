using Game.Utils;
using Game.Actor;
using Game.Loot;
using Game.Database;
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
			mainMenu = GetNode<MainMenu>("c/game_menu");
			mainMenu.Connect("draw", this, nameof(_OnMainMenuDraw));
			mainMenu.Connect("hide", this, nameof(_OnMainMenuHide));
			hudStatus = GetNode<HudStatus>("c/hud_status");
			hud = GetNode<Hud>("c/hud");
			hud.ConnectButtons(this, nameof(_OnHudPausePressed),
				nameof(_OnHudSpellBookPressed), nameof(_OnHudPausePressed));
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
		public void _OnHudSpellBookPressed()
		{
			Globals.PlaySound("click5", this, speaker);
			mainMenu.ShowSpellBook();
		}
		public void ConnectPlayerToHud(Player player)
		{
			player.Connect(nameof(Character.UpdateHudStatus), hudStatus, nameof(HudStatus._OnUpdateStatus));
			hudStatus.UpdateName(true, player.worldName);
			hudStatus._OnUpdateStatus(player, true, player.hp, player.stats.hpMax.valueI);
			hudStatus._OnUpdateStatus(player, false, player.mana, player.stats.manaMax.valueI);
		}
		public void NpcInteract(Npc npc)
		{
			string signal = nameof(Character.UpdateHudStatus);
			string method = nameof(HudStatus._OnUpdateStatus);
			bool interactable = ContentDB.HasContent(npc.Name)
				&& 3 >= Map.Map.map.getAPath(player.GlobalPosition, npc.GlobalPosition).Count;
			if (npc.IsConnected(signal, hudStatus, method))
			{
				if (interactable)
				{
					mainMenu.NpcInteract(npc);
				}
				else
				{
					npc.Disconnect(signal, hudStatus, method);
					hudStatus.npcStatus.Hide();
					player.target = null;
				}
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
				hudStatus._OnUpdateStatus(npc, true, npc.hp, npc.stats.hpMax.valueI);
				hudStatus._OnUpdateStatus(npc, false, npc.mana, npc.stats.manaMax.valueI);
				if (interactable)
				{
					mainMenu.NpcInteract(npc);
				}
				else
				{
					player.target = npc;
				}
			}
		}
		public void LootInteract(LootChest lootChest) { mainMenu.LootInteract(lootChest); }
	}
}