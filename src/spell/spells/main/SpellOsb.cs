using System.Collections.Generic;
using Game.Actor;
using Game.Utils;
using Godot;
namespace Game.Ability
{
    public abstract class OsbSpell : Spell
    {
        Player player;
        List<Character> targets = new List<Character>();
        public override void _Ready()
        {
            SetProcess(false);
        }
        public override void GetPickable(Character character, bool addToBag)
        {
            base.GetPickable(character, addToBag);
            player = (Player)character;
        }

        public override void _Process(float delta)
        {
            GlobalPosition = player.GetCenterPos() + (GetGlobalMousePosition() - player.GetCenterPos()).Clamped(spellRange);
            player.GetMenu().GetNode<Control>("c/osb").
            SetPosition((player.GetCenterPos().y > GlobalPosition.y) ?
                new Vector2(0.0f, 666.0f) :
                new Vector2(0.0f, 180.0f));
        }
        public override void _OnSightAreaEntered(Area2D area2D)
        {
            if (GetNode<Node2D>("img").Visible)
            {
                base._OnSightAreaEntered(area2D);
            }
            else
            {
                Character character = area2D.Owner as  Character;
                if (character != null && !character.IsDead() && !targets.Contains(character))
                {
                    targets.Add(character);
                    if (Visible)
                    {
                        character.Modulate = new Color("#00ff00");
                        character.ZIndex = 1;
                    }
                }
            }
        }
        public override void _OnSightAreaExited(Area2D area2D)
        {
            if (GetNode<Node2D>("img").Visible)
            {
                base._OnSightAreaExited(area2D);
            }
            else
            {
                Character character = area2D.Owner as  Character;
                if (character != null && targets.Contains(character))
                {
                    character.Modulate = new Color("#ffffff");
                    character.ZIndex = 0;
                    targets.Remove(character);
                }
            }
        }
        public override void ConfigureSpell()
        {
            caster.SetCurrentSpell(this);
            caster.SetState(Character.States.IDLE);
            Control osb = player.GetMenu().GetNode<Control>("c/osb");
            osb.SetPosition(new Vector2(0.0f, 180.0f));
            GetNode<CollisionShape2D>("sight/distance").Disabled = false;
            foreach (Spell spell in GetTree().GetNodesInGroup(osb.GetInstanceId().ToString()))
            {
                spell.UnMake();
            }
            AddToGroup(osb.GetInstanceId().ToString());
            caster.GetNode<Tween>("tween").Connect("tween_started", this, nameof(_OnTweenStarted));
            osb.GetNode("m/cast").Connect("pressed", this, nameof(_OnSpellAreaCast));
            osb.GetNode<Label>("m/cast/label").Text = "Cast";
            osb.Show();
            GlobalPosition = caster.GlobalPosition;
            GetNode<Node2D>("spell_area").Show();
            SetProcess(true);
        }
        public void _OnSpellAreaPressed()
        {
            SetProcess(false);
        }
        public void _OnSpellAreaReleased()
        {
            SetProcess(true);
        }
        public void _OnTweenStarted(Godot.Object obj, NodePath nodePath)
        {
            base._OnTweenCompleted(obj, nodePath);
            Player player = obj as Player;
            if (player != null && player.GetState() == Character.States.MOVING &&
                nodePath.Equals(":global_position"))
            {
                player.GetMenu().GetNode<Control>("c/csb").Hide();
                UnMake();
            }
        }
        public void _OnSpellAreaCast()
        {
            SetProcess(false);
            player.GetMenu().GetNode<Control>("c/osb").Hide();
            player.GetMenu().GetNode<Control>("c/osb/m/cast").Disconnect("pressed", this, nameof(_OnSpellAreaCast));
            player.GetNode<Tween>("tween").Disconnect("tween_Started", this, nameof(_OnTweenStarted));
            RemoveFromGroup(player.GetMenu().GetNode("c/osb").GetInstanceId().ToString());
            Globals.PlaySound("click2", this, new Speaker());
            Hide();
            Cast();
            foreach (Character character in targets)
            {
                character.Modulate = new Color("#ffffff");
                character.ZIndex = 0;
            }
        }
    }
}