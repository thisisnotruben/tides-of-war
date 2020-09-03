using Godot;
using Game.Database;
using Game.ItemPoto;
namespace Game.Ui
{
	public class ItemInfoSpellController : ItemInfoController
	{
		public override void _Ready()
		{
			base._Ready();
			GetNode<BaseButton>("s/h/buttons/cast")
				.Connect("pressed", this, nameof(_OnCastPressed));
		}
		public override void Display(string pickableWorldName, bool allowMove)
		{
			base.Display(pickableWorldName, allowMove);

			// disaply cast option if spell not cooling down and player not dead
			HideExcept((Commodity.IsCoolingDown(player, pickableWorldName) || player.dead)
				? new string[] { }
				: new string[] { "cast" });
		}
		public void _OnCastPressed()
		{
			bool showPopup = false;
			SpellDB.SpellNode spellNode = SpellDB.GetSpellData(pickableWorldName);
			Label popupErrLabel = popupController.GetNode<Label>("m/error/label");

			if (player.mana >= spellNode.manaCost)
			{
				if (spellNode.requiresTarget)
				{
					if (player.target == null)
					{
						popupErrLabel.Text = "Target\nRequired!";
						showPopup = true;
					}
					else
					{
						if (!player.target.enemy)
						{
							popupErrLabel.Text = "Invalid\nTarget!";
							showPopup = true;
						}
						else if (player.pos.DistanceTo(player.target.pos) > spellNode.range && spellNode.range > 0)
						{
							popupErrLabel.Text = "Target Not\nIn Range!";
							showPopup = true;
						}
					}
				}
			}
			else
			{
				popupErrLabel.Text = "Not Enough\nMana!";
				showPopup = true;
			}

			if (showPopup)
			{
				GetNode<Control>("s").Hide();
				popupController.GetNode<Control>("m/error").Show();
				popupController.Show();
			}
			else
			{
				Globals.PlaySound("click2", this, speaker);
				// TODO: hmmm...
				// Spell spell = PickableFactory.GetMakeSpell(pickableWorldName);
				// spell.GetPickable(player, false);
				// spell.ConfigureSpell();
				// player.SetCurrentSpell(spell);
				// itemList.SetSlotCoolDown(pickableWorldName, spellNode.coolDown, 0.0f);
				Hide();
			}
		}
	}
}