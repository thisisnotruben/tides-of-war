using System;
using Game.Actor;
using Game.Utils;
using Godot;
namespace Game.Actor.Doodads
{
    public class Grave : Node2D
    {
        private Player deceasedPlayer;
        public void _OnAreaEntered(Area2D area2D)
        {
            if (area2D.Owner == deceasedPlayer)
            {
                deceasedPlayer.GetMenu().GetNode<CanvasItem>("c/osb").Show();
            }
        }
        public void _OnAreaExited(Area2D area2D)
        {
            if (area2D.Owner == deceasedPlayer)
            {
                deceasedPlayer.GetMenu().GetNode<CanvasItem>("c/osb").Hide();
            }
        }
        public void SetDeceasedPlayer(Player deceasedPlayer)
        {
            if (deceasedPlayer != null)
            {
                this.deceasedPlayer = deceasedPlayer;
                this.deceasedPlayer.GetMenu().GetNode<BaseButton>("c/osb/m/cast").Connect("pressed", this, nameof(Revive));
                this.deceasedPlayer.GetMenu().GetNode<Control>("c/osb/").SetPosition(new Vector2(0.0f, 180.0f));
                this.deceasedPlayer.GetMenu().GetNode<Label>("c/osb/m/cast/label").Text = "Revive";
                GlobalPosition = this.deceasedPlayer.GlobalPosition;
                Name = this.deceasedPlayer.worldName;
            }
        }
        public void Revive()
        {
            Globals.PlaySound("click2", this, new Speaker());
            Globals.map.SetVeil(false);
            deceasedPlayer.GetMenu().GetNode<CanvasItem>("c/osb").Hide();
            deceasedPlayer.GetMenu().GetNode<BaseButton>("c/osb/m/cast").Disconnect("pressed", this, nameof(Revive));
            deceasedPlayer.SetState(Character.States.ALIVE);
            Tween tween = GetNode<Tween>("tween");
            tween.InterpolateProperty(this, ":modulate", Modulate,
                new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.75f, Tween.TransitionType.Circ, Tween.EaseType.Out);
            tween.Start();
        }
        public void _OnTweenCompleted()
        {
            QueueFree();
        }
    }
}