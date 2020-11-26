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
		public override void Display(string commodityWorldName, bool allowMove)
		{
			base.Display(commodityWorldName, allowMove);

			// disaply cast option if spell not cooling down and player not dead
			HideExcept(
				Commodity.IsCoolingDown(player.GetPath(), commodityWorldName) || player.dead
					? new Control[] { }
					: new Control[] { castBttn });
		}
		public void _OnCastPressed()
		{
			string errorText = string.Empty;
			SpellDB.SpellData spellData = SpellDB.Instance.GetData(commodityWorldName);

			if (player.mana >= spellData.manaCost)
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
				popupController.ShowError(errorText);
			}
			else
			{
				PlaySound(NameDB.UI.CLICK2);
				// TODO: actually cast spell here
				Hide();
			}
		}
	}
}