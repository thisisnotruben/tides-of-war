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

        public override void Display(string pickableWorldName, bool allowMove)
        {
            base.Display(pickableWorldName, allowMove);
            HideExcept((itemList.IsSlotCoolingDown(pickableWorldName) || player.dead) ? "" : "cast");
        }
        public void _OnCastPressed()
        {
            bool showPopup = false;
            SpellDB.SpellNode spellNode = SpellDB.GetSpellData(pickableWorldName);
            if (player.mana >= spellNode.manaCost)
            {
                if (player.target != null)
                {
                    if (spellNode.requiresTarget && !player.target.enemy)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Invalid\nTarget!";
                        showPopup = true;
                    }
                    else if (player.GetCenterPos().DistanceTo(player.target.GetCenterPos()) > spellNode.spellRange
                    && spellNode.spellRange > 0 && spellNode.requiresTarget)
                    {
                        popup.GetNode<Label>("m/error/label").Text = "Target Not\nIn Range!";
                        showPopup = true;
                    }
                }
                else if (player.target == null && spellNode.requiresTarget)
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
                Spell spell = PickableFactory.GetMakeSpell(pickableWorldName);
                spell.GetPickable(player, false);
                spell.ConfigureSpell();
                player.SetCurrentSpell(spell);
                itemList.SetSlotCoolDown(pickableWorldName, spellNode.coolDown, 0.0f);
                Hide();
            }
        }
    }
}
