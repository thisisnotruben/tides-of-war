using Game.Util;
using Game.Actor.State;
using Godot;
namespace Game.Actor.Doodads
{
	public class Tomb : Node2D
	{
		public static readonly PackedScene scene = (PackedScene)GD.Load("res://src/character/doodads/Tomb.tscn");

		private Player deceasedPlayer;
		private Tween tween;
		private Area2D sight;

		public override void _Ready()
		{
			tween = GetNode<Tween>("tween");
			sight = GetNode<Area2D>("img/sight");
		}
		public void Init(Player deceasedPlayer)
		{
			this.deceasedPlayer = deceasedPlayer;
			deceasedPlayer.menu.GetNode<BaseButton>("c/osb/m/cast").Connect("pressed", this, nameof(Revive));
			deceasedPlayer.menu.GetNode<Control>("c/osb/").SetPosition(new Vector2(0.0f, 180.0f));
			deceasedPlayer.menu.GetNode<Button>("c/osb/m/cast").Text = "Revive";

			// set on map correctly
			GlobalPosition = Map.Map.map.GetGridPosition(deceasedPlayer.GlobalPosition);
		}
		public void OnTweenCompleted(Godot.Object gObject, NodePath key) { QueueFree(); }
		public void _OnAreaEntered(Area2D area2D) { deceasedPlayer.menu.GetNode<CanvasItem>("c/osb").Show(); }
		public void _OnAreaExited(Area2D area2D)
		{
			if (area2D.Owner == deceasedPlayer)
			{
				deceasedPlayer.menu.GetNode<CanvasItem>("c/osb").Hide();
			}
		}
		public void Revive()
		{
			Globals.PlaySound("click2", this, new Speaker());

			// hide button to avoid double click
			sight.Monitoring = false;
			deceasedPlayer.menu.GetNode<CanvasItem>("c/osb").Hide();

			// revive
			Map.Map.map.SetVeil(false);
			deceasedPlayer.state = FSM.State.ALIVE;

			// fade out tomb and delete
			tween.InterpolateProperty(this, ":modulate", Modulate,
				new Color("00ffffff"), 0.75f, Tween.TransitionType.Circ, Tween.EaseType.Out);
			tween.Start();
		}
	}
}