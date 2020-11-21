using Godot;
using Game.Database;
using Game.GameItem;
namespace Game.Ui
{
	public class ItemInfoSpellController : ItemInfoController
	{
		public override void _Ready()
		{
			base._Ready();
			castBttn.Connect("pressed", this, nameof(_OnCastPressed));
		}
		public override void Display(string pickableWorldName, bool allowMove)
		{
			base.Display(pickableWorldName, allowMove);

			// disaply cast option if spell not cooling down and player not dead
			HideExcept(
				Commodity.IsCoolingDown(player.GetPath(), pickableWorldName) || player.dead
					? new Control[] { }
					: new Control[] { castBttn });
		}
		public void _OnCastPressed()
		{
			bool showPopup = false;
			SpellDB.SpellData spellData = SpellDB.Instance.GetData(pickableWorldName);
			Label popupErrLabel = popupController.errorLabel;

			if (player.mana >= spellData.manaCost)
			{
				if (spellData.requiresTarget)
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
						else if (player.pos.DistanceTo(player.target.pos) > spellData.range && spellData.range > 0)
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
				mainContent.Hide();
				popupController.errorView.Show();
				popupController.Show();
			}
			else
			{
				Globals.soundPlayer.PlaySound(NameDB.UI.CLICK2);
				// TODO: actually cast spell here
				Hide();
			}
		}
	}
}