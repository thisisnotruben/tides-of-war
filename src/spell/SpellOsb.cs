using Godot;
using Game.Actor;
using System.Collections.Generic;

namespace Game.Spell
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
        public void _OnSpellAreaPressed()
        {
            SetProcess(false);
        }
        public void _OnSpellAreaReleased()
        {
            SetProcess(true);
        }
        public override void _Process(float delta)
        {
            SetGlobalPosition(player.GetCenterPos() + (GetGlobalMousePosition() - player.GetCenterPos()).Clamped(spellRange));
            player.GetMenu().GetNode<Control>("c/osb").
                SetPosition((player.GetCenterPos().y > GetGlobalPosition().y)
                ? new Vector2(0.0f, 666.0f)
                : new Vector2(0.0f, 180.0f));
        }
        public void _OnTweenStarted(Godot.Object obj, NodePath nodePath)
        {
            base._OnTweenCompleted(obj, nodePath);
            Player player = obj as Player;
            if (player != null && player.GetState() == Character.States.MOVING
            && nodePath.Equals(":global_position"))
            {
                player.GetMenu().GetNode<Control>("c/csb").Hide();
                UnMake();
            }
        }
        public override void _OnSightAreaEntered(Area2D area2D)
        {
            if (GetNode<Node2D>("img").IsVisible())
            {
                base._OnSightAreaEntered(area2D);
            }
            else
            {
                Character character = area2D.GetOwner() as Character;
                if (character != null && !character.IsDead() && !targets.Contains(character))
                {
                    targets.Add(character);
                    if (IsVisible())
                    {
                        character.SetModulate(new Color("#00ff00"));
                        character.SetZIndex(1);
                    }
                }
            }
        }
        public override void _OnSightAreaExited(Area2D area2D)
        {
            if (GetNode<Node2D>("img").IsVisible())
            {
                base._OnSightAreaExited(area2D);
            }
            else
            {
                Character character = area2D.GetOwner() as Character;
                if (character != null && targets.Contains(character))
                {
                    character.SetModulate(new Color("#ffffff"));
                    character.SetZIndex(0);
                    targets.Remove(character);
                }
            }
        }
        public void _OnSpellAreaCast()
        {
            SetProcess(false);
            player.GetMenu().GetNode<Control>("c/osb").Hide();
            player.GetMenu().GetNode<Control>("c/osb/m/cast").Disconnect("pressed", this, nameof(_OnSpellAreaCast));
            player.GetNode<Tween>("tween").Disconnect("tween_Started", this, nameof(_OnTweenStarted));
            RemoveFromGroup(player.GetMenu().GetNode("c/osb").GetInstanceId().ToString());
            Globals.PlaySound("click2", this, new AudioStreamPlayer());
            Hide();
            Cast();
            foreach (Character character in targets)
            {
                character.SetModulate(new Color("#ffffff"));
                character.SetZIndex(0);
            }
        }
    }
}