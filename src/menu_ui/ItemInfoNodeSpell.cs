using Godot;
using Game.Database;
using Game.Ability;
namespace Game.Ui
{
    public class ItemInfoNodeSpell : ItemInfoNode
    {
        public override void _Ready()
        {
            base._Ready();
            GetNode<BaseButton>("s/h/buttons/cast")
                .Connect("pressed", this, nameof(_OnCastPressed));
        }

        public override void Display(Loot.Pickable pickable, bool allowMove)
        {
            base.Display(pickable, allowMove);
            HideExcept((itemList.IsSlotCoolingDown(pickable) || player.dead) ? "" : "cast");
        }
        public void _OnCastPressed()
        {
            bool showPopup = false;
            Spell spell = pickable as Spell;
            if (player.mana >= spell.manaCost)
            {
                if (player.target != null)
                {
                    if (spell.requiresTarget && !player.target.enemy)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Invalid\nTarget!";
                        showPopup = true;
                    }
                    else if (player.GetCenterPos().DistanceTo(player.target.GetCenterPos()) > spell.spellRange
                    && spell.spellRange > 0 && spell.requiresTarget)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Target Not\nIn Range!";
                        showPopup = true;
                    }
                }
                else if (player.target == null && spell.requiresTarget)
                {
                    popup.GetNode<Label>("m/error/label").Text = "Target\nRequired!";
                    showPopup = true;
                }
            }
            else
            {
                popup.GetNode<Label>("m/error/label").Text = "Not Enough\nMana!";
                showPopup = true;
            }
            if (showPopup)
            {
                GetNode<Control>("s").Hide();
                popup.GetNode<Control>("m/error").Show();
                popup.Show();
            }
            else
            {
                Globals.PlaySound("click2", this, speaker);
                spell = PickableFactory.GetMakeSpell(spell.worldName);
                spell.GetPickable(player, false);
                spell.ConfigureSpell();
                player.SetCurrentSpell(spell);
                itemList.SetSlotCoolDown(spell, spell.cooldown, 0.0f);
                Hide();
            }
        }
    }
}
