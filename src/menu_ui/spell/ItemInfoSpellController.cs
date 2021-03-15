using Godot;
using Game.Database;
using Game.GameItem;
using Game.Factory;
using Game.Ability;
using Game.Actor.State;
namespace Game.Ui
{
	public class ItemInfoSpellController : ItemInfoController
	{
		[Signal] public delegate void PlayerWantstoCast(Spell spell, bool requiresTarget);
		[Signal] public delegate void PlayerWantstoCastError(string errotText);

		public override void _Ready()
		{
			base._Ready();
			castBttn.Connect("pressed", this, nameof(OnCastPressed));
			CallDeferred(nameof(InitSetPlayerCastSpell));
		}
		private void InitSetPlayerCastSpell()
		{
			Connect(nameof(PlayerWantstoCast), player.fsm, nameof(FSM.OnPlayerWantsToCast));
		}
		public override void Display(string commodityWorldName, bool allowMove)
		{
			base.Display(commodityWorldName, allowMove);

			// disaply cast option if spell not cooling down and player not dead
			HideExcept(
				Commodity.IsCoolingDown(player.GetPath(), commodityWorldName) || player.dead
					? new Control[] { }
					: new Control[] { castBttn });
		}
		public void OnCastPressed()
		{
			string errorText = string.Empty;
			SpellDB.SpellData spellData = Globals.spellDB.GetData(commodityWorldName);

			if (player.dead)
			{
				errorText = "Can't Cast\nWhile Dead!";
			}
			else if (Commodity.IsCoolingDown(player.GetPath(), commodityWorldName))
			{
				errorText = "Cooling\nDown!";
			}
			else if (player.mana >= spellData.manaCost)
			{
				if (spellData.requiresTarget)
				{
					if (player.target == null)
					{
						errorText = "Target\nRequired!";
					}
					else
					{
						if (!player.target.enemy)
						{
							errorText = "Invalid\nTarget!";
						}
						else if (player.pos.DistanceTo(player.target.pos) > spellData.range && spellData.range > 0)
						{
							errorText = "Target Not\nIn Range!";
						}
					}
				}
			}
			else
			{
				errorText = "Not Enough\nMana!";
			}

			if (!errorText.Equals(string.Empty))
			{
				mainContent.Hide();
				popup.ShowError(errorText);
				EmitSignal(nameof(PlayerWantstoCastError), errorText);
			}
			else
			{
				PlaySound(NameDB.UI.CLICK2);
				Hide();

				Spell spell = new SpellFactory().Make(player, commodityWorldName);
				spell.Connect(nameof(Spell.OnStarted), this, nameof(OnCasted));

				EmitSignal(nameof(PlayerWantstoCast), spell,
					Globals.spellDB.GetData(commodityWorldName).requiresTarget);
			}
		}
		public void OnCasted(string spellName) { CheckHudSlots(inventoryModel, spellName); }
	}
}